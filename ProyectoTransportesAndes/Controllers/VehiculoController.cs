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

        /// <summary>
        /// 
        /// </summary>
        /// <returns>vista con la lista de vehiculos que hay en el sistema</returns>
        [HttpGet]
        [Route("Index")]
        [ActionName("Index")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> IndexAsync()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    var vehiculos = await _controladoraVehiculos.getVehiculos();
                    return View(vehiculos);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index","Home");
            }
            catch (Exception)
            {
                TempData["Error"] = "Se produjo un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>vista para ingresar un nuevo vehiculo</returns>
        [HttpGet]
        [Route("Nuevo")]
        [AutoValidateAntiforgeryToken]
        public IActionResult Nuevo()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    ViewModelVehiculo model = new ViewModelVehiculo(_settings);
                    return View(model);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "Se produjo un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// recibe el modelo con los datos del nuevo vehiculo y lo ingresa al sistema
        /// </summary>
        /// <param name="model"></param>
        /// <returns>vista index</returns>
        [HttpPost]
        [Route("Nuevo")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> NuevoAsync(ViewModelVehiculo model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    model.Vehiculo.Tipo = model.TipoVehiculo;
                    await _controladoraVehiculos.nuevoVehiculo(model.Vehiculo, model.ChoferSeleccionado);
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Se produjo un error inesperado. Intente de nuevo mas tarde");
                return View(model);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>vista para editar el vehiculo seleccionado</returns>
        [HttpGet]
        [Route("Edit")]
        [ActionName("Edit")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> EditAsync(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    Vehiculo vehiculo = await _controladoraVehiculos.getVehiculoBaseDatos(id);
                    ViewModelVehiculo model = new ViewModelVehiculo(_settings);
                    model.Vehiculo = vehiculo;
                    model.Id = vehiculo.Id.ToString();
                    if (vehiculo.Chofer == null)
                    {
                        model.Vehiculo.Chofer = new Chofer();
                    }
                    return View(model);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "Se produjo un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// modifica el vehiculo segun los datos del modelo pasado
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns>vista index</returns>
        [HttpPost]
        [Route("Edit")]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(string id, ViewModelVehiculo model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {

                    await _controladoraVehiculos.editarVehiculo(model.Vehiculo, id, model.ChoferSeleccionado, model.TipoVehiculo);
                    return RedirectToAction("Index");

                }
                else
                {
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Se produjo un error inesperado. Intente de nuevo mas tarde");
                return View(model);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>vista para eliminar el vehiculo seleccionado</returns>
        [HttpGet]
        [Route("Delete")]
        [ActionName("Delete")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    var vehiculo = await _controladoraVehiculos.getVehiculoBaseDatos(id);
                    return View(vehiculo);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "Se produjo un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// elimina el vehiculo seleccionado
        /// </summary>
        /// <param name="id"></param>
        /// <param name="vehiculo"></param>
        /// <returns>vista index</returns>
        [HttpPost]
        [Route("Delete")]
        [ActionName("Delete")]
        [AutoValidateAntiforgeryToken]
        public async Task<ActionResult> DeleteAsync(string id, Vehiculo vehiculo)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    await _controladoraVehiculos.eliminarVehiculo(id);
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View(vehiculo);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Se produjo un error inesperado. Intente de nuevo mas tarde");
                return View(vehiculo);
            }
        }

        #endregion
    }
}