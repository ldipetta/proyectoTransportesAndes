using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Exceptions;
using ProyectoTransportesAndes.Models;
using ProyectoTransportesAndes.Persistencia;
using ProyectoTransportesAndes.ViewModels;

namespace ProyectoTransportesAndes.Controllers
{
    [Produces("application/json")]
    [Route("api/Vehiculo")]
    public class VehiculoController : Controller
    {
        #region Atributos
        private IOptions<AppSettingsMongo> _settings;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;
        private ControladoraVehiculos _controladoraVehiculos;
        #endregion

        #region Constructores
        public VehiculoController(IOptions<AppSettingsMongo> settings, IHttpContextAccessor httpContextAccessor)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _httpContext = httpContextAccessor;
            _settings = settings;
            _controladoraVehiculos = ControladoraVehiculos.getInstance(_settings);
        }
        #endregion

        #region Acciones

        [HttpGet]
        [Route("Index")]
        [ActionName("Index")]
        public async Task<IActionResult> IndexAsync()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    var vehiculos = await _controladoraVehiculos.getVehiculos();
                    return View(vehiculos);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No posee los permisos. Inicie sesión");
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Se produjo un error inesperado. Intente de nuevo mas tarde");
                return View();
            }
        }

        [HttpGet]
        [Route("Nuevo")]
        [ActionName("Nuevo")]
        public IActionResult Nuevo()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    ViewModelVehiculo model = new ViewModelVehiculo(_settings);
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No posee los permisos. Inicie sesión");
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Se produjo un error inesperado. Intente de nuevo mas tarde");
                return View();
            }
        }

        [HttpPost]
        [Route("Nuevo")]
        [ActionName("Nuevo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevoAsync(ViewModelVehiculo model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    if (ModelState.IsValid)
                    {
                        model.Vehiculo.Tipo = model.TipoVehiculo;
                        await _controladoraVehiculos.nuevoVehiculo(model.Vehiculo, model.ChoferSeleccionado);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return View(model.Vehiculo);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No posee los permisos. Inicie sesión");
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Se produjo un error inesperado. Intente de nuevo mas tarde");
                return View();
            }
        }

        [HttpGet]
        [Route("Edit")]
        [ActionName("Edit")]
        public IActionResult EditAsync(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    Vehiculo vehiculo = _controladoraVehiculos.getVehiculo(id);
                    ViewModelVehiculo model = new ViewModelVehiculo(_settings);
                    model.Vehiculo = vehiculo;
                    model.Id = vehiculo.Id.ToString();
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No posee los permisos. Inicie sesión");
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Se produjo un error inesperado. Intente de nuevo mas tarde");
                return View();
            }
        }

        [HttpPost]
        [Route("Edit")]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(string id, ViewModelVehiculo model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    if (ModelState.IsValid)
                    {
                        await _controladoraVehiculos.editarVehiculo(model.Vehiculo, id,model.ChoferSeleccionado,model.TipoVehiculo);
                        return RedirectToAction("Index");
                    }
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No posee los permisos. Inicie sesión");
                    return RedirectToAction("Login");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Se produjo un error inesperado. Intente de nuevo mas tarde");
                return View();
            }
        }

        [HttpGet]
        [Route("Delete")]
        [ActionName("Delete")]
        public IActionResult DeleteAsync(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    var vehiculo = _controladoraVehiculos.getVehiculo(id);
                    return View(vehiculo);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No posee los permisos. Inicie sesión");
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Se produjo un error inesperado. Intente de nuevo mas tarde");
                return View();
            }
        }

        [HttpPost]
        [Route("Delete")]
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(string id, Vehiculo vehiculo)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    await _controladoraVehiculos.eliminarVehiculo(id, vehiculo);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No posee los permisos. Inicie sesión");
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Se produjo un error inesperado. Intente de nuevo mas tarde");
                return View();
            }
        }

        #endregion
    }
}