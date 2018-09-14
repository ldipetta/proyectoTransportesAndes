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
            List<Usuario> items = new List<Usuario>();
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
                    return RedirectToAction("Login");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View(items);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                return View(items);
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
                if (Seguridad.validarUsuarioAdministrador(token))
                {
                    return View();
                }
                else
                {
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
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                return View();
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
        public async Task<IActionResult> Create(ViewModelUsuario model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrador(token))
                {
                    if (ModelState.IsValid)
                    {
                        if (model.ConfirmarPassword.Equals(model.Usuario.Password))
                        {
                            Usuario usuario = await _controladoraUsuarios.CrearAdministrativo(model.Usuario, model.Administrador);
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
                    ModelState.AddModelError(string.Empty, "No tiene permisos necesarios. Inicie sesión");
                    return RedirectToAction("Login");
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
                    if (id == null)
                    {
                        ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        Usuario usuario = await _controladoraUsuarios.getAdministrativo(id);
                        if (usuario != null)
                        {
                            return View(usuario);
                        }
                        return RedirectToAction("Index");
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
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                return View();
            }
        }
        /// <summary>
        /// elimina el administrativo pasado
        /// </summary>
        /// <param name="id"></param>
        /// <param name="usuario"></param>
        /// <returns>la vista index</returns>
        [HttpPost]
        [Route("Delete")]
        [ActionName("Delete")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Delete(string id, Usuario usuario)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrador(token))
                {
                    await _controladoraUsuarios.EliminarAdministrativo(usuario, id);
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, "No posee los permisos. Inicie sesión");
                return RedirectToAction("Login");

            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View(usuario);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                return View(usuario);
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
                    Usuario usuario = await _controladoraUsuarios.getAdministrativo(id);
                    if (usuario == null)
                    {
                        ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, intente de nuevo mas tarde");
                        return RedirectToAction("Index");
                    }
                    ViewModelUsuario editar = new ViewModelUsuario();
                    editar.Usuario = usuario;
                    editar.Id = usuario.Id.ToString();
                    if (usuario.Tipo == "Administrador")
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
                    ModelState.AddModelError(string.Empty, "No posee los permisos. Inicie sesión");
                    return RedirectToAction("Login");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "He ocurrido un error inesperado, intentelo de nuevo mas tarde");
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
        public async Task<IActionResult> Edit(string id, ViewModelUsuario model)
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
                            model.Usuario.Tipo = "Administrador";
                        }
                        else
                        {
                            model.Usuario.Tipo = "Administrativo";
                        }
                        await _controladoraUsuarios.ModificarAdministrativo(model.Usuario, model.Id);
                        return RedirectToAction("Index");
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