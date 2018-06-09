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
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Models;
using ProyectoTransportesAndes.Persistencia;
using ProyectoTransportesAndes.ViewModels;

namespace ProyectoTransportesAndes.Controllers
{
    [Produces("application/json")]
    [Route("api/Account")]
    public class AccountController : Controller
    {
        private string coleccion = "Usuarios";
        private IOptions<AppSettingsMongo> _settings;
        private readonly IConfiguration _configuration;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;

        public AccountController(IOptions<AppSettingsMongo> settings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _configuration = configuration;
            _httpContext = httpContextAccessor;
            _settings = settings;

            DBRepositoryMongo<Usuario>.Iniciar(_settings);
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
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    var items = await DBRepositoryMongo<Usuario>.GetItemsAsync(coleccion);
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

        [Route("Create")]
        [HttpGet]
        public ActionResult Create() {
            return View();
        }

        [Route("Create")]
        [HttpPost]
        public async Task<ActionResult> Create(ViewModelUsuario view)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (ModelState.IsValid)
                    {
                        Usuario usuario = await DBRepositoryMongo<Usuario>.GetUsuario(view.Usuario.User, coleccion);
                        if (usuario.User == view.Usuario.User)
                        {
                            return BadRequest("El usuario ya existe");
                        }
                        else
                        {
                            if (usuario.Email == view.Usuario.Email)
                            {
                                return BadRequest("El email ya se encuentra registrado");
                            }
                            else
                            {
                                if (view.Administrador)
                                {
                                    Usuario admin = new Administrador();
                                    admin = view.Usuario;
                                    admin.Tipo = "Administrador";
                                    await DBRepositoryMongo<Usuario>.Create(admin, coleccion);
                                    return RedirectToAction("Index");
                                }
                                else
                                {
                                    Usuario admin = new Administrativo();
                                    admin = view.Usuario;
                                    admin.Tipo = "Administrativo";
                                    await DBRepositoryMongo<Usuario>.Create(admin, coleccion);
                                    return RedirectToAction("Index");
                                }
                            }
                        }
                    }
                    else
                    {
                        return BadRequest(ModelState);
                    }
                }
                else
                {
                    return BadRequest("Debe iniciar sesión");
                }
            }
            else
            {
                return BadRequest("No posee los permisos");
            }
    }

        [HttpGet]
        [Route("Login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult> Login(Usuario model)
        {
            if (ModelState.IsValid)
            {
                Usuario user =await DBRepositoryMongo<Usuario>.Login(model.User, model.Password);
                if (user != null)
                {
                    if (user.Password == model.Password)
                    {
                        _session.SetString("Token", Usuario.BuildToken(user, _configuration));
                        _session.SetString("User", model.User);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return BadRequest("Contraseña incorrecta");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpGet]
        [Route("NuevoCliente")]
        public ActionResult NuevoCliente()
        {
            return View();
        }
        [HttpPost]
        [Route("NuevoCliente")]
        public async Task<ActionResult>NuevoCliente(ViewModelCliente model)
        {
            if (ModelState.IsValid)
            {
                Usuario usuario = await DBRepositoryMongo<Usuario>.GetUsuario(model.Usuario.User, coleccion);
                if (!usuario.User.Equals(model.Usuario.User))
                {
                    if (!usuario.Email.Equals(model.Usuario.Email))
                    {
                        Cliente cliente = new Cliente();
                        cliente.Tipo = "Cliente";
                        TarjetaDeCredito tarjeta = new TarjetaDeCredito();
                        tarjeta.fVencimiento = model.Tarjeta.fVencimiento;
                        tarjeta.Numero = model.Tarjeta.Numero;
                        cliente.Tarjeta = tarjeta;
                        await DBRepositoryMongo<Usuario>.Create(cliente, coleccion);
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        return BadRequest("el email ya se encuentra registrado");
                    }
                }
                else
                {
                    return BadRequest("El usuario ya existe");
                }
            }
            else
            {
                return BadRequest(ModelState);
            } 
        }

        [Route("Salir")]
        public IActionResult Salir()
        {
            var session = _session.GetString("Token");
            if (session != null)
            {
                _session.SetString("Token", "");
            }
            return RedirectToAction("Index", "Home");
        }
    }
}