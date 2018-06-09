using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Models;
using ProyectoTransportesAndes.Persistencia;

namespace ProyectoTransportesAndes.Controllers
{
    [Produces("application/json")]
    [Route("api/Vehiculo")]
    public class VehiculoController : Controller
    {
        private string coleccion = "Vehiculos";
        private IOptions<AppSettingsMongo> _settings;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;
        public VehiculoController(IOptions<AppSettingsMongo> settings,IHttpContextAccessor httpContextAccessor)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _httpContext = httpContextAccessor;
            _settings = settings;
            DBRepositoryMongo<Vehiculo>.Iniciar(_settings);
        }

        [HttpGet]
        [Route("Index")]
        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync()
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol=="Administrativo")
                {
                    var items = await DBRepositoryMongo<Vehiculo>.GetItemsAsync(coleccion);
                    return View(items);
                }
                else
                {
                    return BadRequest("No posee los permisos");
                }
            }
            else
            {
                return BadRequest("Debe iniciar sesión");
            }
        }

        [HttpGet]
        [Route("Nuevo")]
        [ActionName("Nuevo")]
        public IActionResult Nuevo()
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    return View();
                }
                else
                {
                    return BadRequest("No posee los permisos");
                }
            }
            else
            {
                return BadRequest("Debe iniciar sesión");
            }
        }

        [HttpPost]
        [Route("Nuevo")]
        [ActionName("Nuevo")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NuevoAsync(Vehiculo vehiculo)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (ModelState.IsValid)
                    {
                        await DBRepositoryMongo<Vehiculo>.Create(vehiculo, coleccion);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return View(vehiculo);
                    }
                }
                else
                {
                    return BadRequest("No posee los permisos");
                }
            }
            else
            {
                return BadRequest("Debe iniciar sesión");
            }
           
        }

        [HttpGet]
        [Route("Edit")]
        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(string id)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                        if (id == null)
                        {
                            return BadRequest();
                        }
                        Vehiculo item = await DBRepositoryMongo<Vehiculo>.GetItemAsync(id, coleccion);
                        if (item == null)
                        {
                            return NotFound();
                        }
                        return View(item);
                }
                else
                {
                    return BadRequest("No posee los permisos");
                }
            }
            else
            {
                return BadRequest("Debe iniciar sesión");
            }
           
        }

        [HttpPost]
        [Route("Edit")]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync(string id, Vehiculo vehiculo)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (ModelState.IsValid)
                    {
                        vehiculo.Id = new ObjectId(id);
                        await DBRepositoryMongo<Vehiculo>.UpdateAsync(vehiculo.Id, vehiculo, coleccion);
                        return RedirectToAction("Index");
                    }
                    return View(vehiculo);
                }
                else
                {
                    return BadRequest("No posee los permisos");
                }
            }
            else
            {
                return BadRequest("Debe iniciar sesión");
            }
            
        }

        [HttpGet]
        [Route("Delete")]
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (id == null)
                    {
                        return BadRequest();
                    }
                    Vehiculo item = await DBRepositoryMongo<Vehiculo>.GetItemAsync(id, coleccion);
                    if (item == null)
                    {
                        return NotFound();
                    }
                    return View(item);
                }
                else
                {
                    return BadRequest("No posee los permisos");
                }
            }
            else
            {
                return BadRequest("Debe iniciar sesión");
            }
        }

        [HttpPost]
        [Route("Delete")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAsync(string id, Vehiculo vehiculo)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (ModelState.IsValid)
                    {
                        vehiculo.Id = new ObjectId(id);
                        await DBRepositoryMongo<Vehiculo>.DeleteAsync(vehiculo.Id, coleccion);
                        return RedirectToAction("Index");
                    }
                    return View(vehiculo);
                }
                else
                {
                    return BadRequest("No posee los permisos");
                }
            }
            else
            {
                return BadRequest("Debe iniciar sesión");
            }
        }
    }
}