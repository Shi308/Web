using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace Lab2.Controllers
{
    public class HelloWorldController : Controller
    {
        // GET: /HelloWorld/
        public string Index()
        {
            return "This is my default action...";
        }

        // GET: /HelloWorld/Welcome/
        public string Welcome(string name, int numtimes = 1)
        {
            return HtmlEncoder.Default.Encode($"Hello {name}, NumTimes is: {numtimes}");
        }
    }
}
