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

        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    var items = await _controladoraUsuarios.getChoferes();
                    return View(items);
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
        [Route("Create")]
        public IActionResult Create()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    return View();
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

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(ViewModelChofer model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    if (ModelState.IsValid)
                    {
                        Chofer chofer = await _controladoraUsuarios.CrearChofer(model.Chofer, model.Libreta);
                        if (chofer != null)
                        {
                            return RedirectToAction("Index", "Chofer");
                        }
                        return View(model);
                    }
                    else
                    {
                        return View(model);
                    }
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
        [Route("Edit")]
        [ActionName("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    Chofer chofer = await _controladoraUsuarios.getChofer(id);
                    if (chofer == null)
                    {
                        ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, intente de nuevo mas tarde");
                        return RedirectToAction("Index");
                    }
                    ViewModelChofer editar = new ViewModelChofer();
                    editar.Chofer = chofer;
                    editar.Libreta = chofer.LibretaDeConducir;
                    editar.Id = chofer.Id.ToString();
                    return View(editar);
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

        [HttpPost]
        [Route("Edit")]
        [ActionName("Edit")]
        public async Task<ActionResult> Edit(string id, ViewModelChofer model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    await _controladoraUsuarios.ModificarChofer(model.Chofer, model.Id);
                    return RedirectToAction("Index");
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
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
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

        [HttpPost]
        [Route("Delete")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id, Chofer chofer)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    if (ModelState.IsValid)
                    {

                        await _controladoraUsuarios.EliminarChofer(chofer, id);
                        return RedirectToAction("Index");
                    }
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

        #endregion
    }
}