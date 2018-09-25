using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    [Route("api/Chofer")]
    public class ChoferController : Controller
    {
        #region Atributos

        private IOptions<AppSettingsMongo> _settings;
        private readonly IConfiguration _configuration;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;
        private ControladoraUsuarios _controladoraUsuarios;

        #endregion

        #region Constructores

        public ChoferController(IOptions<AppSettingsMongo> settings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _configuration = configuration;
            _httpContext = httpContextAccessor;
            _settings = settings;
            _controladoraUsuarios = ControladoraUsuarios.getInstance(_settings);
        }

        #endregion

        #region Acciones

        /// <summary>
        /// carga los choferes de la bd en la vista
        /// </summary>
        /// <returns>vista index</returns>
        [HttpGet]
        [Route("Index")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Index()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    var items = await _controladoraUsuarios.getChoferes();
                    return View(items);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index", "Home");
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
        /// <returns>vista create</returns>
        [HttpGet]
        [Route("Create")]
        [AutoValidateAntiforgeryToken]
        public IActionResult Create()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    return View();
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
        /// crea un nuevo chofer segun modelo
        /// </summary>
        /// <param name="model"></param>
        /// <returns>vista index</returns>
        [HttpPost]
        [Route("Create")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(ViewModelChofer model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    if (ModelState.IsValid)
                    {
                        if (model.ConfirmarContraseña.Equals(model.Chofer.Password))
                        {
                            Chofer chofer = await _controladoraUsuarios.CrearChofer(model.Chofer, model.Libreta);
                            if (chofer != null)
                            {
                                return RedirectToAction("Index");
                            }
                            return View(model);
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Las contraseñas deben coincidir");
                            return View(model);
                        }
                    }
                    else
                    {
                        return View(model);
                    }
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
        /// carga el chofer seleccionado
        /// </summary>
        /// <param name="id"></param>
        /// <returns>vista edit</returns>
        [HttpGet]
        [Route("Edit")]
        [ActionName("Edit")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    Chofer chofer = await _controladoraUsuarios.getChofer(id);
                    if (chofer == null)
                    {
                        ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, intente de nuevo mas tarde");
                        return RedirectToAction("Index");
                    }
                    ViewModelChofer editar = new ViewModelChofer();
                    editar.Chofer = chofer;
                    editar.Chofer.Password = chofer.Password;
                    editar.Libreta = chofer.LibretaDeConducir;
                    editar.Id = chofer.Id.ToString();
                    return View(editar);
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
        /// modifica el chofer seleccionado
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns>vista index</returns>
        [HttpPost]
        [Route("Edit")]
        [ActionName("Edit")]
        [AutoValidateAntiforgeryToken]
        public async Task<ActionResult> Edit(string id, ViewModelChofer model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    if (ModelState.IsValid)
                    {
                        await _controladoraUsuarios.ModificarChofer(model.Chofer, model.Id);
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

        /// <summary>
        /// carga la vista con el chofer a eliminar
        /// </summary>
        /// <param name="id"></param>
        /// <returns>vista delete</returns>
        [HttpGet]
        [Route("Delete")]
        [ActionName("Delete")]
        [AutoValidateAntiforgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    Chofer chofer = await _controladoraUsuarios.getChofer(id);
                    return View(chofer);
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

        /// <summary>
        /// elimina el chofer seleccionado
        /// </summary>
        /// <param name="id"></param>
        /// <param name="chofer"></param>
        /// <returns>vista index</returns>
        [HttpPost]
        [Route("Delete")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id, Chofer chofer)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    await _controladoraUsuarios.EliminarChofer(chofer, id);
                    return RedirectToAction("Index");
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

        #endregion
    }
}