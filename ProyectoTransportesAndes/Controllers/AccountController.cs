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
            DBRepositoryMongo<Cliente>.Iniciar(_settings);
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

        //Crea un nuevo usuario administrador o administrativo
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
                        if (usuario!=null)
                        {
                            return BadRequest("El usuario ya existe");
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
                    else
                    {
                        return BadRequest(ModelState);
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
                Usuario user =await DBRepositoryMongo<Usuario>.Login(model.User, model.Password,"Usuarios");
                if (user == null)
                {
                    Cliente cliente = await DBRepositoryMongo<Cliente>.Login(model.User, model.Password, "Clientes");
                    if (cliente != null)
                    {
                        if (cliente.Password == model.Password)
                        {
                            _session.SetString("Token", Usuario.BuildToken(cliente/*, _configuration*/));
                            _session.SetString("UserTipo", cliente.Tipo);
                            _session.SetString("UserName", cliente.Nombre);
                            _session.SetString("UserId", cliente.Id.ToString());
                            _session.SetString("Session", "si");
                         
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
                    if (user.Password == model.Password)
                    {
                        _session.SetString("Token", Usuario.BuildToken(user/*, _configuration*/));
                        _session.SetString("UserTipo", user.Tipo);
                        _session.SetString("UserName", user.Nombre);
                        _session.SetString("UserId", user.Id.ToString());
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return BadRequest("Contraseña incorrecta");
                    }
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
                _session.Clear();
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("Delete")]
        [ActionName("Delete")]
        public async Task<ActionResult> Delete(string id)
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
                    Usuario item = await DBRepositoryMongo<Usuario>.GetItemAsync(id, coleccion);
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
        public async Task<ActionResult> Delete(string id, Usuario usuario)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (ModelState.IsValid)
                    {
                        usuario.Id = new ObjectId(id);
                        await DBRepositoryMongo<Usuario>.DeleteAsync(usuario.Id, coleccion);
                        return RedirectToAction("Index");
                    }
                    return View(usuario);
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
        public async Task<ActionResult> Edit(string id)
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
                    Usuario item = await DBRepositoryMongo<Usuario>.GetItemAsync(id, coleccion);
                    if (item == null)
                    {
                        return NotFound();
                    }
                    ViewModelUsuario editar = new ViewModelUsuario();
                    editar.Usuario = item;
                    editar.Id = item.Id.ToString();
                    if (item.Tipo == "Administrador")
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
        public async Task<ActionResult> Edit(string id, ViewModelUsuario model)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (ModelState.IsValid)
                    {
                        model.Usuario.Id = new ObjectId(id);
                        await DBRepositoryMongo<Usuario>.UpdateAsync(model.Usuario.Id, model.Usuario, coleccion);
                        return RedirectToAction("Index");
                    }
                    return View(model.Usuario);
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