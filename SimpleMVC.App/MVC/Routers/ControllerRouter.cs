using SimpleMVC.App.MVC.Interfaces;
using System.Collections.Generic;
using SimpleHttpServer.Models;
using System.Linq;
using System;
using System.Reflection;
using SimpleMVC.App.MVC.Attributes.Methods;
using SimpleMVC.App.MVC.Controllers;
using SimpleHttpServer.Enums;
using System.Net;

namespace SimpleMVC.App.MVC.Routers
{
    public class ControllerRouter : IHandleable
    {
        private IDictionary<string, string> getParams;
        private IDictionary<string, string> postParams;
        private string requestMethod;
        private string controllerName;
        private string actionName;
        private object[] methodParams;

        

        public HttpResponse Handle(HttpRequest request)
        {
            string url = WebUtility.UrlDecode(request.Url);
            string query = string.Empty;
            if (url.Contains('?'))
            {
                query = url.Split('?')[1];
            }

            var controllerActionParams = url.Split('?');
            var controllerAction = controllerActionParams[0].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            controllerActionParams = query.Split('&');

            if (controllerActionParams.Length >= 1)
            {
                foreach (var getPar in controllerActionParams)
                {
                    if (getPar.Contains('='))
                    {
                        var keyValue = getPar.Split('=');
                        getParams.Add(keyValue[0], keyValue[1]);
                    }
                }
            }

            string contentParam = request.Content;

            if (contentParam != null)
            {
                contentParam = WebUtility.UrlDecode(contentParam);
                var postRequestParams = contentParam.Split('&');

                foreach (var postPar in postRequestParams)
                {
                    var parametersRest = postPar.Split('=');
                    postParams.Add(parametersRest[0], parametersRest[1]);
                }
            }

            this.requestMethod = request.Method.ToString();

            string controllerNameFormat = controllerAction[0].First().ToString().ToUpper() + String.Join("", controllerAction[0].Skip(1)) + "Controller";
            this.controllerName = controllerNameFormat;

            string actionNameFormat = controllerAction[1].First().ToString().ToUpper() + String.Join("", controllerAction[1].Skip(1));
            this.actionName = actionNameFormat;

            MethodInfo method = this.GetMethod();

            if (method == null)
            {
                throw new NotSupportedException("No such method");
            }

            IEnumerable<ParameterInfo> parameters = method.GetParameters();

            this.methodParams = new object[parameters.Count()];

            int index = 0;

            foreach (ParameterInfo param in parameters)
            {
                if (param.ParameterType.IsPrimitive)
                {
                    object value = this.getParams[param.Name];
                    this.methodParams[index] = Convert.ChangeType(value, param.ParameterType);
                    index++;
                }
                else
                {
                    Type bindingModelType = param.ParameterType;
                    object bindingModel = Activator.CreateInstance(bindingModelType);
                    IEnumerable<PropertyInfo> properties = bindingModelType.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        property.SetValue(bindingModel, Convert.ChangeType(postParams[property.Name], property.PropertyType));
                    }

                    this.methodParams[index] = Convert.ChangeType(bindingModel, bindingModelType);
                    index++;
                }
            }

            IInvocable actionResult = (IInvocable)this.GetMethod()
                .Invoke(this.GetController(), this.methodParams);

            string content = actionResult.Invoke();
            var response = new HttpResponse()
            {
                StatusCode = ResponseStatusCode.Ok,
                ContentAsUTF8 = content
            };

            this.ClearParamDictionaries();
            return response;
        }

        private MethodInfo GetMethod()
        {
            MethodInfo method = null;

            foreach (MethodInfo methodInfo in this.GetSuitableMethods())
            {
                IEnumerable<Attribute> attributes = methodInfo
                    .GetCustomAttributes()
                    .Where(a => a is HttpMethodAttribute);

                if (!attributes.Any())
                {
                    return methodInfo;
                }

                foreach (HttpMethodAttribute attribute in attributes)
                {
                    if (attribute.IsValid(this.requestMethod))
                    {
                        return methodInfo;
                    }
                }
            }

            return method;
        }

        private IEnumerable<MethodInfo> GetSuitableMethods()
        {
            return this.GetController()
                .GetType()
                .GetMethods()
                .Where(m => m.Name == this.actionName);
        }

        private Controller GetController()
        {
            var controllerType = string.Format(
                "{0}.{1}.{2}",
                MvcContext.Current.AssemblyName,
                MvcContext.Current.ControllersFolder,
                this.controllerName);

            var controller = (Controller)Activator.CreateInstance(Type.GetType(controllerType));

            return controller;
        }

        private void ClearParamDictionaries()
        {
            this.getParams = new Dictionary<string, string>();
            this.postParams = new Dictionary<string, string>();
        }
    }
}
