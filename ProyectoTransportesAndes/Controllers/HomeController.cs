using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProyectoTransportesAndes.Models;
using Microsoft.AspNetCore.Http;

namespace ProyectoTransportesAndes.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;

        public HomeController( IHttpContextAccessor httpContextAccessor)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _httpContext = httpContextAccessor;
        }
        [HttpGet]
        [ActionName("Index")]
        public IActionResult Index()
        {
            if (_session.GetString("User") == null)
            {
                _session.SetString("User", "Cliente");
                _session.SetString("UserName", "");
                return View();
            }
            else
            {
                return View();
            }
            
            
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
    }
}
