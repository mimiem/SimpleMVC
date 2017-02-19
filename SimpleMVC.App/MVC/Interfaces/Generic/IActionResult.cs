using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMVC.App.MVC.Interfaces.Generic
{
    public interface IActionResult<T> : IInvocable
    {
        IRenderable<T> Action { get; set; }
    }
}
