using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Exceptions;
using ProyectoTransportesAndes.Models;
using ProyectoTransportesAndes.Persistencia;
using ProyectoTransportesAndes.ViewModels;

namespace ProyectoTransportesAndes.Controllers
{
    [Produces("application/json")]
    [Route("api/Account")]
    public class AccountController : Controller
    {
        #region Atributos
        private IOptions<AppSettingsMongo> _settings;
        private readonly IConfiguration _configuration;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;
        private ControladoraUsuarios _controladoraUsuarios;
        #endregion

        #region Constructores
        public AccountController(IOptions<AppSettingsMongo> settings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _configuration = configuration;
            _httpContext = httpContextAccessor;
            _settings = settings;
            DBRepositoryMongo<Cliente>.Iniciar(_settings);
            DBRepositoryMongo<Usuario>.Iniciar(_settings);
            _controladoraUsuarios = ControladoraUsuarios.getInstance(_settings);
        }
        #endregion

        #region Acciones

        [HttpGet]
        [Route("Index")]
        [ActionName("Index")]
        public async Task<IActionResult> IndexAsync()
        {
            List<Usuario> items = new List<Usuario>();
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    items = (List<Usuario>)await _controladoraUsuarios.getAdministrativos();
                    return View(items);
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty,msg.Message);
                return View(items);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                return View(items);
            }
        }

        [Route("Create")]
        [HttpGet]
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
                    return RedirectToAction("Login");
                }
            }
            catch(MensajeException msg)
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

        //Crea un nuevo usuario administrador o administrativo
        [Route("Create")]
        [HttpPost]
        public async Task<IActionResult> Create(ViewModelUsuario view)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    if (ModelState.IsValid)
                    {
                     Usuario usuario = await _controladoraUsuarios.CrearAdministrativo(view.Usuario, view.Administrador);
                        if (usuario != null)
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            return View(view);
                        }
                    }
                    else
                    {
                        return View(view);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene permisos necesarios. Inicie sesión");
                    return RedirectToAction("Login");
                }
            }catch(MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View(view);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                return View(view);
            }
        }

        [HttpGet]
        [Route("Login")]
        public IActionResult Login()
        {
            try
            {
                return View();
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado");
                return View();
            }
           
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(ViewModelLogin model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Usuario user = await _controladoraUsuarios.Login(model.Usuario, model.Password);
                    if (user != null)
                    {
                        _session.SetString("Token", Usuario.BuildToken(user));
                        _session.SetString("UserTipo", user.Tipo);
                        _session.SetString("UserName", user.Nombre);
                        _session.SetString("UserId", user.Id.ToString());
                        _session.SetString("Session", "si");
                        return RedirectToAction("Index", "Home");
                    }
                    return View();
                }
                ModelState.AddModelError(string.Empty, "Error al iniciar sesión");
                return View();
            }
            catch (MensajeException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View();
            }

        }

        [Route("Salir")]
        public IActionResult Salir()
        {
            try
            {
                var session = _session.GetString("Token");
                if (session != null)
                {
                    _session.Clear();
                }
                return RedirectToAction("Index", "Home");
            }catch(MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado");
                return RedirectToAction("Index", "Home");
            }
           
        }

        [HttpGet]
        [Route("Delete")]
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    if (id == null)
                    {
                        ModelState.AddModelError(string.Empty,"Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        Usuario usuario = await _controladoraUsuarios.getAdministrativo(id);
                        if ( usuario != null)
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
            catch(MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View();
            }
            catch(Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                return View();
            }
        }

        [HttpPost]
        [Route("Delete")]
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(string id, Usuario usuario)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    if (ModelState.IsValid)
                    {
                       await _controladoraUsuarios.EliminarAdministrativo(usuario, id);
                        return RedirectToAction("Index");
                    }
                    return View(usuario);
                }
                    ModelState.AddModelError(string.Empty, "No posee los permisos. Inicie sesión");
                    return RedirectToAction("Login");
      
            }
            catch(MensajeException msg)
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
            catch(MensajeException msg)
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

        [HttpPost]
        [Route("Edit")]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ViewModelUsuario model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    if (ModelState.IsValid)
                    {
                        await _controladoraUsuarios.EliminarAdministrativo(model.Usuario, model.Id);
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
            catch(MensajeException msg)
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