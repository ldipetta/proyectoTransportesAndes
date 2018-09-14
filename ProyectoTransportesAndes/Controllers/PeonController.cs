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

namespace ProyectoTransportesAndes.Controllers
{
    [Produces("application/json")]
    [Route("api/Peon")]
    public class PeonController : Controller
    {
        private string coleccion = "Peones";
        private IOptions<AppSettingsMongo> _settings;
        private readonly IConfiguration _configuration;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;

        public PeonController(IOptions<AppSettingsMongo> settings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _configuration = configuration;
            _httpContext = httpContextAccessor;
            _settings = settings;
            DBRepositoryMongo<Peon>.Iniciar(_settings);
        }
        [HttpGet]
        [Route("Index")]
        [ActionName("Index")]
        public async Task<ActionResult> Index()
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Seguridad.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    List<Peon> peones = await ControladoraUsuarios.getInstance(_settings).getPeones();
                    return View(peones);
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
        public ActionResult Create()
        {
            return View();
        }

        [Route("Create")]
        [HttpPost]
        public async Task<ActionResult> Create(Peon model)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Seguridad.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (ModelState.IsValid)
                    {
                        Peon peon = await DBRepositoryMongo<Peon>.GetPeon(model.Documento, coleccion);
                        if (peon != null)
                        {
                            return BadRequest("El peon ya existe");
                        }
                        else
                        {
                            model = model.Encriptar(model);
                            await DBRepositoryMongo<Peon>.Create(model, coleccion);
                            return RedirectToAction("Index");
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
        [Route("Edit")]
        [ActionName("Edit")]
        public async Task<ActionResult> Edit(string id)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Seguridad.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (id == null)
                    {
                        return BadRequest();
                    }
                    Peon item = await ControladoraUsuarios.getInstance(_settings).getPeon(id);
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
        [Route("Edit")]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, Peon peon)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Seguridad.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (ModelState.IsValid)
                    {
                        peon.Id = new ObjectId(id);
                        peon = peon.Encriptar(peon);
                        await DBRepositoryMongo<Peon>.UpdateAsync(peon.Id, peon, coleccion);
                        return RedirectToAction("Index");
                    }
                    return View(peon);
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
                var rol = Seguridad.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (id == null)
                    {
                        return BadRequest();
                    }
                    Peon item = await ControladoraUsuarios.getInstance(_settings).getPeon(id);
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
        public async Task<ActionResult>Delete(string id, Peon peon)
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Seguridad.validarToken(token);
                if (rol == "Administrador" || rol == "Administrativo")
                {
                    if (ModelState.IsValid)
                    {
                        await DBRepositoryMongo<Peon>.DeleteAsync(id, coleccion);
                        return RedirectToAction("Index");
                    }
                    return View(peon);
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