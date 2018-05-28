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
    public class VehiculoController : Controller
    {
        private string coleccion = "Vehiculos";
        private IOptions<AppSettings> _settings;

        public VehiculoController(IOptions<AppSettings> settings)
        {
            _settings = settings;
            DBRepository<Vehiculo>.Iniciar(_settings);
        }
        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync()
        {
            var items = await DBRepository<Vehiculo>.GetItemsAsync(coleccion);
            return View(items);
        }
        [HttpGet]
        [ActionName("Nuevo")]
        public IActionResult Nuevo()
        {
            return View();
        }
        [HttpPost]
        [ActionName("Nuevo")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NuevoAsync(Vehiculo vehiculo)
        {
            if (ModelState.IsValid)
            {
                await DBRepository<Vehiculo>.Create(vehiculo, coleccion);
                return RedirectToAction("Index");
            }
            return View(vehiculo);
        }
        [HttpGet]
        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Vehiculo item = await DBRepository<Vehiculo>.GetItemsAsync(id,coleccion);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }
        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync(Vehiculo vehiculo)
        {
            if (ModelState.IsValid)
            {
                await DBRepository<Vehiculo>.UpdateItemsAsync(vehiculo.Id, vehiculo,coleccion);
                return RedirectToAction("Index");
            }
            return View(vehiculo);
        }
        [HttpGet]
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Vehiculo item = await DBRepository<Vehiculo>.GetItemsAsync(id, coleccion);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAsync(Vehiculo vehiculo)
        {
            if (ModelState.IsValid)
            {
                await DBRepository<Vehiculo>.DeleteItemsAsync(vehiculo.Id,coleccion);
                return RedirectToAction("Index");
            }
            return View(vehiculo);
        }
    }
}