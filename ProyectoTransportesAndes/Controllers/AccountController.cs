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

        /// <summary>
        /// 
        /// </summary>
        /// <returns>vista para ingresar al sistema</returns>
        [HttpGet]
        [Route("Login")]
        [AutoValidateAntiforgeryToken]
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns>vista index del home</returns>
        [HttpPost]
        [AutoValidateAntiforgeryToken]
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
                        _session.SetString("Token", Seguridad.BuildToken(user));
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

        /// <summary>
        /// sale del sistema
        /// </summary>
        /// <returns>vista index de home</returns>
        [AutoValidateAntiforgeryToken]
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
                //ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado");
                return RedirectToAction("Index", "Home");
            }
           
        }
        #endregion
    }
}