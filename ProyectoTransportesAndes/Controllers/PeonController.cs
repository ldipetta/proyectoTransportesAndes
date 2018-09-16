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

namespace ProyectoTransportesAndes.Controllers
{
    [Produces("application/json")]
    [Route("api/Peon")]
    public class PeonController : Controller
    {
        #region Atributos
        private IOptions<AppSettingsMongo> _settings;
        private readonly IConfiguration _configuration;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;
        #endregion

        #region Constructores
        public PeonController(IOptions<AppSettingsMongo> settings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _configuration = configuration;
            _httpContext = httpContextAccessor;
            _settings = settings;
            DBRepositoryMongo<Peon>.Iniciar(_settings);
        }
        #endregion

        #region Acciones

        /// <summary>
        /// carga en la vista todos los peones ingresados
        /// </summary>
        /// <returns>vista index</returns>
        [HttpGet]
        [Route("Index")]
        [ActionName("Index")]
        [AutoValidateAntiforgeryToken]
        public async Task<ActionResult> Index()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    List<Peon> peones = await ControladoraUsuarios.getInstance(_settings).getPeones();
                    return View(peones);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado, intente de nuevo mas tarde";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// vista para crear un nuevo peon
        /// </summary>
        /// <returns>vista create</returns>
        [Route("Create")]
        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public ActionResult Create()
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
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado, intente de nuevo mas tarde";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// ingresa un nuevo peon al sistema
        /// </summary>
        /// <param name="model"></param>
        /// <returns>vista index</returns>
        [Route("Create")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<ActionResult> Create(Peon model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    if (ModelState.IsValid)
                    {
                        await ControladoraUsuarios.getInstance(_settings).CrearPeon(model);
                        return RedirectToAction("Index");
                    }
                        return View(model);
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
                TempData["Error"] = "Ha ocurrido un error inesperado, intente de nuevo mas tarde";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>vista edit</returns>
        [HttpGet]
        [Route("Edit")]
        [ActionName("Edit")]
        [AutoValidateAntiforgeryToken]
        public async Task<ActionResult> Edit(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    Peon p = await ControladoraUsuarios.getInstance(_settings).getPeon(id);
                    return View(p);
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
                TempData["Error"] = "Ha ocurrido un error inesperado, intente de nuevo mas tarde";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// vista con el peon seleccionado para editar
        /// </summary>
        /// <param name="id"></param>
        /// <param name="peon"></param>
        /// <returns>vista edit</returns>
        [HttpPost]
        [Route("Edit")]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, Peon peon)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    if (ModelState.IsValid)
                    {
                       await ControladoraUsuarios.getInstance(_settings).ModificarPeon(peon,id);
                        return RedirectToAction("Index");
                    }
                    return View(peon);
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
                TempData["Error"] = "Ha ocurrido un error inesperado, intente de nuevo mas tarde";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>vista delete</returns>
        [HttpGet]
        [Route("Delete")]
        [ActionName("Delete")]
        [AutoValidateAntiforgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    Peon p = await ControladoraUsuarios.getInstance(_settings).getPeon(id);
                    return View(p);
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
                TempData["Error"] = "Ha ocurrido un error inesperado, intente de nuevo mas tarde";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// vista cargada con el peon solicitado para eliminar
        /// </summary>
        /// <param name="id"></param>
        /// <param name="peon"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Delete")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id, Peon peon)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    if (ModelState.IsValid)
                    {
                        await ControladoraUsuarios.getInstance(_settings).EliminarPeon(id);
                        return RedirectToAction("Index");
                    }
                    return View(peon);
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
                TempData["Error"] = "Ha ocurrido un error inesperado, intente de nuevo mas tarde";
                return RedirectToAction("Index");
            }
        }
        #endregion
    }
}