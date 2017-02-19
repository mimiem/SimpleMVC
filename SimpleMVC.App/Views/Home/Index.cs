using SimpleMVC.App.MVC.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMVC.App.Views.Home
{
    public class Index : IRenderable
    {
        public string Render()
        {
            return "<h3>Hello MVC </h4>";
        }
    }
}
