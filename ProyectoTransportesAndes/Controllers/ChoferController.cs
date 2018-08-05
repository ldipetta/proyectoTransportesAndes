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
    [Route("api/Chofer")]
    public class ChoferController : Controller
    {
        private string coleccion = "Choferes";
        private IOptions<AppSettingsMongo> _settings;
        private readonly IConfiguration _configuration;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;

        public ChoferController(IOptions<AppSettingsMongo> settings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _configuration = configuration;
            _httpContext = httpContextAccessor;
            _settings = settings;

            DBRepositoryMongo<Chofer>.Iniciar(_settings);
        }

        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index() {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    var items = await DBRepositoryMongo<Chofer>.GetItemsAsync(coleccion);
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
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("Create")]
        public async Task<ActionResult> Create(ViewModelChofer model)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (ModelState.IsValid)
                    {
                        Chofer chofer = await DBRepositoryMongo<Chofer>.GetUsuario(model.Chofer.User, coleccion);
                        if (chofer == null)
                        {
                            Chofer nuevo = new Chofer();
                            nuevo = model.Chofer;
                            nuevo.Tipo = "Chofer";
                            nuevo.Disponible = true;
                            LibretaDeConducir libreta = new LibretaDeConducir();
                            libreta.FVencimiento = model.Libreta.FVencimiento;
                            libreta.Categoria = model.Libreta.Categoria;
                            nuevo.LibretaDeConducir = libreta;
                            nuevo.Leyenda = model.Chofer.Numero + " - " + model.Chofer.Nombre + " " + model.Chofer.Apellido;
                            await DBRepositoryMongo<Chofer>.Create(nuevo, coleccion);
                            return RedirectToAction("Index", "Chofer");
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
                    Chofer item = await DBRepositoryMongo<Chofer>.GetItemAsync(id, coleccion);
                    if (item == null)
                    {
                        return NotFound();
                    }
                    ViewModelChofer editar = new ViewModelChofer();
                    editar.Chofer = item;
                    editar.Libreta = item.LibretaDeConducir;
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
        public async Task<ActionResult> Edit(string id, ViewModelChofer model)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (ModelState.IsValid)
                    {
                        model.Chofer.Id = new ObjectId(id);
                        model.Chofer.LibretaDeConducir = model.Libreta;
                        await DBRepositoryMongo<Chofer>.UpdateAsync(model.Chofer.Id, model.Chofer, coleccion);
                        return RedirectToAction("Index");
                    }
                    return View(model.Chofer);
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
                    Chofer item = await DBRepositoryMongo<Chofer>.GetItemAsync(id, coleccion);
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
        public async Task<ActionResult> Delete(string id, Chofer chofer)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (ModelState.IsValid)
                    {
                        chofer.Id = new ObjectId(id);
                        await DBRepositoryMongo<Chofer>.DeleteAsync(chofer.Id, coleccion);
                        return RedirectToAction("Index");
                    }
                    return View(chofer);
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