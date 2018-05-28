using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Models;
using ProyectoTransportesAndes.Persistencia;

namespace ProyectoTransportesAndes.Controllers
{
    public class AdministradorController : Controller
    {
        private string coleccion = "Administradores";
        private IOptions<AppSettings> _settings;

        public AdministradorController(IOptions<AppSettings>settings) {
            _settings = settings;
            DBRepository<Administrador>.Iniciar(_settings);
        }
        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync()
        {
            var items = await DBRepository<Administrador>.GetItemsAsync(coleccion);
            return View(items);
        }
        [HttpGet]
        [ActionName("Create")]
        public IActionResult Nuevo()
        {
            return View();
        }
        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NuevoAsync(Administrador administrador)
        {
            if (ModelState.IsValid)
            {
                await DBRepository<Administrador>.Create(administrador,coleccion);
                return RedirectToAction("Index");
            }
            return View(administrador);
        }


    }
}