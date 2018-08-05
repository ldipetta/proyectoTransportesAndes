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
using ProyectoTransportesAndes.ViewModels;

namespace ProyectoTransportesAndes.Controllers
{
    [Produces("application/json")]
    [Route("api/Vehiculo")]
    public class VehiculoController : Controller
    {
        private IOptions<AppSettingsMongo> _settings;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;
        private ControladoraVehiculos _controladoraVehiculos;
        public VehiculoController(IOptions<AppSettingsMongo> settings,IHttpContextAccessor httpContextAccessor)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _httpContext = httpContextAccessor;
            _settings = settings;
            _controladoraVehiculos = ControladoraVehiculos.getInstance(_settings);
        }

        [HttpGet]
        [Route("Index")]
        [ActionName("Index")]
        public async Task<IActionResult> IndexAsync()
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol=="Administrativo")
                {
                    var vehiculos = await _controladoraVehiculos.getVehiculos();
                    return View(vehiculos);
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
                    ViewModelVehiculo model = new ViewModelVehiculo(_settings);
                    return View(model);
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
        public async Task<IActionResult> NuevoAsync(ViewModelVehiculo model)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (ModelState.IsValid)
                    {
                        Vehiculo nuevo = model.Vehiculo;
                        Chofer chofer = await _controladoraVehiculos.getChofer(model.ChoferSeleccionado);
                        chofer.Disponible = false;
                        nuevo.Chofer = chofer;
                        nuevo.Disponible = true;
                        nuevo.Unidades = _controladoraVehiculos.calcularUnidades(nuevo.Largo,nuevo.Ancho,nuevo.Alto);
                        await _controladoraVehiculos.nuevoVehiculo(nuevo);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return View(model.Vehiculo);
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
                        Vehiculo item = await _controladoraVehiculos.getVehiculo(id);
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
                       await _controladoraVehiculos.editarVehiculo(vehiculo, id);
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
        public async Task<IActionResult> DeleteAsync(string id)
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
                    var item = await _controladoraVehiculos.getVehiculo(id);
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
                        
                        await _controladoraVehiculos.eliminarVehiculo(vehiculo, id);
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