using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProyectoTransportesAndes.Models;
using Microsoft.AspNetCore.Http;
using ProyectoTransportesAndes.Exceptions;

namespace ProyectoTransportesAndes.Controllers
{
    public class HomeController : Controller
    {
        #region Atributos
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;
        #endregion

        #region Constructores
        public HomeController( IHttpContextAccessor httpContextAccessor)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _httpContext = httpContextAccessor;
        }
        #endregion

        #region Acciones
        [HttpGet]
        [ActionName("Index")]
        public IActionResult Index()
        {
            try
            {
                if (_session.GetString("UserTipo") == null)
                {
                    _session.SetString("UserTipo", "Cliente");
                    _session.SetString("UserName", "");
                    _session.SetString("Session", "no");
                    return View();
                }
                else
                {
                    return View();
                }
            }catch(MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado");
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
        #endregion
    }
}
