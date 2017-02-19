using SimpleHttpServer;
using SimpleMVC.App.MVC;
using SimpleMVC.App.MVC.Routers;

namespace SimpleMVC.App
{
    class AppStart
    {
        static void Main()
        {
            HttpServer server = new HttpServer(8081, RouteTable.Routes);
            MvcEngine.Run(server);
        }
    }
}
