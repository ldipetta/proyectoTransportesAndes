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
using ProyectoTransportesAndes.ViewModels;

namespace ProyectoTransportesAndes.Controllers
{
    [Produces("application/json")]
    [Route("api/Cliente")]
    public class ClienteController : Controller
    {
        #region Atributos
        private IOptions<AppSettingsMongo> _settings;
        private readonly IConfiguration _configuration;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;
        private ControladoraUsuarios _controladoraUsuarios;
        #endregion

        #region Constructores
        public ClienteController(IOptions<AppSettingsMongo> settings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _configuration = configuration;
            _httpContext = httpContextAccessor;
            _settings = settings;
            _controladoraUsuarios = ControladoraUsuarios.getInstance(_settings);
        }
        #endregion

        #region Acciones

        /// <summary>
        /// carga los usaurios administrativos en la vista
        /// </summary>
        /// <returns>vista index</returns>
        [HttpGet]
        [Route("Index")]
        [AutoValidateAntiforgeryToken]
        [ActionName("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    var items = await _controladoraUsuarios.getClientes();
                    return View(items);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index","Home");
            }
            catch (Exception)
            {
                TempData["Error"] = "Se produjo un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns>devuelve la vista cargada con el ViewModelCliente</returns>
        [HttpGet]
        [Route("Create")]
        [AutoValidateAntiforgeryToken]
        public IActionResult Create()
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
                ModelState.AddModelError(string.Empty, "Se produjo un error inesperado. Intente de nuevo mas tarde");
                return View();
            }
        }
       
        /// <summary>
        /// si el modelo es valido, crea un nuevo cliente
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Create")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(ViewModelCliente model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.ConfirmarPassword.Equals(model.Cliente.Password))
                    {
                        await _controladoraUsuarios.CrearCliente(model.Cliente, model.Tarjeta);
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Deben coincidir las contraseñas");
                        return View(model);
                    }

                }
                else
                {
                    return View(model);
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Se produjo un error inesperado. Intente de nuevo mas tarde");
                return View(model);
            }
        }

        /// <summary>
        /// carga en la vista el cliente seleccionado para eliminar
        /// </summary>
        /// <param name="id"></param>
        /// <returns>vista para eliminar el usuario seleccionado</returns>
        [HttpGet]
        [Route("Delete")]
        [ActionName("Delete")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    Cliente cliente = await _controladoraUsuarios.getCliente(id);
                    return View(cliente);
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
                TempData["Error"] = "Se produjo un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// elimina al usuario seleccionado
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cliente"></param>
        /// <returns>vista index</returns>
        [HttpPost]
        [Route("Delete")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, Cliente cliente)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    await _controladoraUsuarios.EliminarCliente(cliente, id);
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View(cliente);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Se produjo un error inesperado. Intente de nuevo mas tarde");
                return View(cliente);
            }
        }

        /// <summary>
        /// carga en la vista el cliente seleccionado
        /// </summary>
        /// <param name="id"></param>
        /// <returns>vista para editar al usuario seleccionado</returns>
        [HttpGet]
        [Route("Edit")]
        [ActionName("Edit")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    Cliente cliente = await _controladoraUsuarios.getCliente(id);
                    ViewModelCliente editar = new ViewModelCliente();
                    editar.Cliente = cliente;
                    editar.Tarjeta = cliente.Tarjeta;
                    editar.Id = cliente.Id.ToString();
                    return View(editar);
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
                TempData["Error"] = "Se produjo un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// modifica al cliente seleccionado
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns>vista index</returns>
        [HttpPost]
        [Route("Edit")]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ViewModelCliente model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    model.Tarjeta = new TarjetaDeCredito() { fVencimiento = DateTime.Now.ToShortDateString(), Numero = "" };
                    model.Cliente.Ubicacion = new PosicionSatelital();
                    model.Cliente.Password = "";
                    model.Cliente.Documento = "";
                    await _controladoraUsuarios.ModificarCliente(model.Cliente, id, model.Tarjeta);
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Se produjo un error inesperado. Intente de nuevo mas tarde");
                return View(model);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>vista para ingresar un cliente desde el backend</returns>
        [HttpGet]
        [Route("Ingresar")]
        [AutoValidateAntiforgeryToken]
        public IActionResult Ingresar()
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
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "Se produjo un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// si el modelo es valido, crea un nuevo cliente
        /// </summary>
        /// <param name="model"></param>
        /// <returns>vista index</returns>
        [HttpPost]
        [Route("Ingresar")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Ingresar(ViewModelCliente model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    model.Tarjeta = new TarjetaDeCredito();
                    model.Cliente.Password = "";
                    model.Cliente.FNacimiento = DateTime.Now.ToShortDateString();
                    model.Cliente.Documento = "";
                    if (!string.IsNullOrEmpty(model.Cliente.RazonSocial))
                    {
                        model.Cliente.User = model.Cliente.RazonSocial;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(model.Cliente.Nombre))
                        {
                            model.Cliente.User = model.Cliente.Nombre;
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Debe ingresar la razon social o el nombre");
                            return View(model);
                        }
                    }
                    await _controladoraUsuarios.CrearCliente(model.Cliente, model.Tarjeta);
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Se produjo un error inesperado. Intente de nuevo mas tarde");
                return View(model);
            }
        }

        #endregion
    }
}