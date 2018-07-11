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
using ProyectoTransportesAndes.Models;
using ProyectoTransportesAndes.Persistencia;
using ProyectoTransportesAndes.ViewModels;

namespace ProyectoTransportesAndes.Controllers
{
    [Produces("application/json")]
    [Route("api/Cliente")]
    public class ClienteController : Controller
    {
        private string coleccion = "Clientes";
        private IOptions<AppSettingsMongo> _settings;
        private readonly IConfiguration _configuration;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;

        public ClienteController(IOptions<AppSettingsMongo> settings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _configuration = configuration;
            _httpContext = httpContextAccessor;
            _settings = settings;
            DBRepositoryMongo<Cliente>.Iniciar(_settings);
            DBRepositoryMongo<Usuario>.Iniciar(_settings);
            DBRepositoryMongo<Chofer>.Iniciar(_settings);
        }

        [HttpGet]
        [Route("Index")]
        [ActionName("Index")]
        public async Task<ActionResult> Index()
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    var items = await DBRepositoryMongo<Cliente>.GetItemsAsync(coleccion);
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

        [HttpGet]
        [Route("Create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("Create")]
        public async Task<ActionResult> Create(ViewModelCliente model)
        {
            if (ModelState.IsValid)
            {
                Usuario usuario = await DBRepositoryMongo<Usuario>.GetUsuario(model.Cliente.User, "Usuarios");
                Cliente cliente = await DBRepositoryMongo<Cliente>.GetUsuario(model.Cliente.User, coleccion);
                Chofer chofer = await DBRepositoryMongo<Chofer>.GetUsuario(model.Cliente.User, "Choferes");
                if (usuario == null && cliente==null && chofer==null)
                {
                    Cliente nuevo = new Cliente();
                    nuevo = model.Cliente;
                    nuevo.Tipo = "Cliente";
                    TarjetaDeCredito tarjeta = new TarjetaDeCredito();
                    tarjeta.fVencimiento = model.Tarjeta.fVencimiento;
                    tarjeta.Numero = model.Tarjeta.Numero;
                    nuevo.Tarjeta = tarjeta;
                    await DBRepositoryMongo<Cliente>.Create(nuevo, coleccion);
                    return RedirectToAction("Login", "Account");

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
                    Cliente item = await DBRepositoryMongo<Cliente>.GetItemAsync(id, coleccion);
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
        public async Task<ActionResult> Delete(string id, Cliente cliente)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (ModelState.IsValid)
                    {
                        cliente.Id = new ObjectId(id);
                        await DBRepositoryMongo<Cliente>.DeleteAsync(cliente.Id, coleccion);
                        return RedirectToAction("Index");
                    }
                    return View(cliente);
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
                    Cliente item = await DBRepositoryMongo<Cliente>.GetItemAsync(id, coleccion);
                    if (item == null)
                    {
                        return NotFound();
                    }
                    ViewModelCliente editar = new ViewModelCliente();
                    editar.Cliente = item;
                    editar.Tarjeta = item.Tarjeta;
                    editar.Id = item.Id.ToString();
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
        public async Task<ActionResult> Edit(string id, ViewModelCliente model)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (ModelState.IsValid)
                    {
                        model.Cliente.Id = new ObjectId(id);
                        model.Cliente.Tarjeta = model.Tarjeta;
                        await DBRepositoryMongo<Cliente>.UpdateAsync(model.Cliente.Id, model.Cliente, coleccion);
                        return RedirectToAction("Index");
                    }
                    return View(model.Cliente);
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