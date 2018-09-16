using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Exceptions;
using ProyectoTransportesAndes.Models;
using ProyectoTransportesAndes.Persistencia;
using ProyectoTransportesAndes.ViewModels;

namespace ProyectoTransportesAndes.Controllers
{
    [Produces("application/json")]
    [Route("api/Administrativo")]
    public class AdministrativoController : Controller
    {
        #region Atributos
        private IOptions<AppSettingsMongo> _settings;
        private readonly IConfiguration _configuration;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;
        private ControladoraUsuarios _controladoraUsuarios;
        #endregion

        #region Constructores
        public AdministrativoController(IOptions<AppSettingsMongo> settings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _configuration = configuration;
            _httpContext = httpContextAccessor;
            _settings = settings;
            DBRepositoryMongo<Usuario>.Iniciar(_settings);
            _controladoraUsuarios = ControladoraUsuarios.getInstance(_settings);
        }
        #endregion

        #region Acciones

        /// <summary>
        /// 
        /// </summary>
        /// <returns>vista con todos los administrativos cargados</returns>
        [HttpGet]
        [Route("Index")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Index()
        {
            List<Administrativo> items = new List<Administrativo>();
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    items = await _controladoraUsuarios.getAdministrativos();
                    return View(items);
                }
                else
                {
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>vista con un view model usuario</returns>
        [Route("Create")]
        [HttpGet]
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
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// recibe el view model cargado, lo valida y sii crea el usuario
        /// </summary>
        /// <param name="model"></param>
        /// <returns>retorna a la vista login si salio todo ok</returns>
        [Route("Create")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(ViewModelAdministrativo model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    if (ModelState.IsValid)
                    {
                        if (model.ConfirmarPassword.Equals(model.Administrativo.Password))
                        {
                            Usuario usuario = await _controladoraUsuarios.CrearAdministrativo(model.Administrativo, model.Administrador);
                            return RedirectToAction("Index");
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
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                return View(model);
            }
        }

        /// <summary>
        /// recibe un id de un administrativo y lo busca para cargarlo en la vista
        /// </summary>
        /// <param name="id"></param>
        /// <returns>vista cargada con el usuario pasado</returns>
        [HttpGet]
        [Route("Delete")]
        [ActionName("Delete")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrador(token))
                {
                    Administrativo administrativo = await _controladoraUsuarios.getAdministrativo(id);
                    if (administrativo != null)
                    {
                        return View(administrativo);
                    }
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
                TempData["Error"] = "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// elimina el administrativo pasado
        /// </summary>
        /// <param name="id"></param>
        /// <param name="administrativo"></param>
        /// <returns>la vista index</returns>
        [HttpPost]
        [Route("Delete")]
        [ActionName("Delete")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Delete(string id, Administrativo administrativo)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrador(token))
                {
                    await _controladoraUsuarios.EliminarAdministrativo(administrativo, id);
                    return RedirectToAction("Index");
                }
                return RedirectToAction("Login","Account");
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View(administrativo);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                return View(administrativo);
            }
        }

        /// <summary>
        /// carga el administrativo pasado en la vista para editarlo
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Edit")]
        [ActionName("Edit")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrador(token))
                {
                    Administrativo admin = await _controladoraUsuarios.getAdministrativo(id);
                    if (admin == null)
                    {
                        return RedirectToAction("Index");
                    }
                    ViewModelAdministrativo editar = new ViewModelAdministrativo();
                    editar.Administrativo = admin;
                    editar.Id = admin.Id.ToString();
                    if (admin.Tipo == "Administrador")
                    {
                        editar.Administrador = true;
                    }
                    else
                    {
                        editar.Administrador = false;
                    }
                    return View(editar);
                }
                else
                {
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "He ocurrido un error inesperado, intentelo de nuevo mas tarde";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// edita el administrativo pasado desde la vista
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns>la vista index</returns>
        [HttpPost]
        [Route("Edit")]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ViewModelAdministrativo model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrador(token))
                {
                    if (ModelState.IsValid)
                    {
                        if (model.Administrador)
                        {
                            model.Administrativo.Tipo = "Administrador";
                        }
                        else
                        {
                            model.Administrativo.Tipo = "Administrativo";
                        }
                        await _controladoraUsuarios.ModificarAdministrativo(model.Administrativo, model.Id);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return View(model);
                    }
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
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                return View(model);
            }
        }

        #endregion
    }
}