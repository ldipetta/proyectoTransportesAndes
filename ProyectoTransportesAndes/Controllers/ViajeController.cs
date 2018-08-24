using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Models;
using ProyectoTransportesAndes.Persistencia;
using ProyectoTransportesAndes.ViewModels;
using ProyectoTransportesAndes.ControllersAPI;
using ProyectoTransportesAndes.Exceptions;

namespace ProyectoTransportesAndes.Controllers
{
    [Produces("application/json")]
    [Route("api/Viaje")]
    public class ViajeController : Controller
    {
        #region Atributos
        private IOptions<AppSettingsMongo> _settings;
        private readonly IConfiguration _configuration;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;
        private ControladoraViajes _controladoraViajes;
        private ControladoraVehiculos _controladoraVehiculos;
        private ControladoraUsuarios _controladoraUsuarios;
        #endregion

        #region Constructores
        public ViajeController(IOptions<AppSettingsMongo> settings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _configuration = configuration;
            _httpContext = httpContextAccessor;
            _settings = settings;
            _controladoraViajes = ControladoraViajes.getInstancia(_settings);
            _controladoraVehiculos = ControladoraVehiculos.getInstance(_settings);
            _controladoraUsuarios = ControladoraUsuarios.getInstance(_settings);
        }
        #endregion

        #region Acciones
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            ViewModelViajeFiltro model = new ViewModelViajeFiltro(_settings);
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    model.Viajes = await _controladoraViajes.getViajes();
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No posee los permisos. Inicie sesión");
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
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, intente de nuevo mas tarde");
                return View(model);
            }
        }

        [HttpPost]
        [Route("Index")]
        public async Task<IActionResult> Index(ViewModelViajeFiltro model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    var viajes = await _controladoraViajes.getViajes();

                    if (!model.IdCliente.Equals("000000000000000000000000"))
                    {
                        Cliente cliente = await _controladoraUsuarios.getCliente(model.IdCliente);
                        viajes.Where(v => v.Cliente.Equals(cliente));
                    }
                    if (!model.IdVehiculo.Equals("0"))
                    {
                        Vehiculo vehiculo = await _controladoraVehiculos.getVehiculo(model.IdVehiculo);
                        viajes.Where(v => v.Vehiculo.Equals(vehiculo));
                    }
                    if (!model.EstadoViaje.Equals(EstadoViaje.Estado))
                    {
                        viajes.Where(v => v.Estado.Equals(model.EstadoViaje));
                    }
                    if (model.Desde != null)
                    {
                        viajes.Where(v => v.Fecha >= model.Desde);
                    }
                    if (model.Hasta != null)
                    {
                        viajes.Where(v => v.Fecha <= model.Hasta);
                    }

                    model.Viajes = viajes;
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos. Inice sesión");
                    return RedirectToAction("Login", "Account");
                }

            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, intente de nuevo mas tarde");
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Route("Nuevo")]
        public IActionResult Nuevo()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    ViewModelViajeDirecto model = new ViewModelViajeDirecto(_settings);
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene permisos. Inicie sesión");
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Route("Nuevo")]
        public async Task<IActionResult> Nuevo(ViewModelViajeDirecto model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    await _controladoraViajes.nuevoViaje(model.VehiculoId,model.ClienteId,model.Direccion,model.Fecha,model.HoraInicio,model.Comentarios);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene permisos. Inicie sesión");
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return View(model);
            }
        }

        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    Viaje viaje = await _controladoraViajes.getViaje(id);
                    return View(viaje);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene persmisos. Inicie sesión");
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id, Viaje viaje)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    if (ModelState.IsValid)
                    {
                        await _controladoraViajes.modificarViaje(id, viaje);
                        return RedirectToAction("Index");
                    }
                    return View(viaje);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene persmisos. Inicie sesión");
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    Viaje viaje = await _controladoraViajes.getViaje(id);
                    return View(viaje);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene persmisos. Inicie sesión");
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string id, Viaje viaje)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    await _controladoraViajes.eliminarViaje(id, viaje);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene persmisos. Inicie sesión");
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpGet]
        [Route("SolicitarViaje")]
        public async Task<IActionResult> SolicitarViaje()
        {
            ViewModelViaje model = new ViewModelViaje();
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioCliente(token))
                {
                    string cliente = _session.GetString("UserId");
                    if (!cliente.Equals(""))
                    {
                        var viajePendienteCliente = await _controladoraViajes.viajePendienteCliente(cliente);
                        var vehiculos = _controladoraVehiculos.vehiculosConPosicion();
                        model.Viaje = viajePendienteCliente;
                        if (model.Viaje != null)
                        {
                            model.IdViaje = viajePendienteCliente.Id.ToString();
                        }
                        else
                        {
                            ViewData["Mensaje"] = "No tiene items ingresados";
                        }
                        model.Vehiculos = await vehiculos;
                        if (model.Viaje == null)
                        {
                            model.Viaje = new Viaje();
                            ViewData["Mensaje"] = "No tiene items ingresados";
                            model.Viaje.Items = new List<Item>();
                        }
                        return View(model);
                    }
                    return View(model);
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
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error desconocido, vuelva a intentarlo mas tarde");
                return View(model);
            }
        }

        [HttpPost]
        [Route("SolicitarViaje")]
        public async Task<IActionResult> SolicitarViaje(string idViaje, string direccionDestino, bool viajeMarcado)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioCliente(token))
                {
                    if (viajeMarcado)
                    {
                        if (direccionDestino == null)
                        {
                            ModelState.AddModelError(string.Empty, "Debe seleccionar una direccion de destino");
                            return RedirectToAction("SolicitarViaje");
                        }
                    }
                    string idCliente = _session.GetString("UserId");
                    if (idViaje != null)
                    {
                        Viaje viaje = await _controladoraViajes.solicitarViaje(idViaje, idCliente, direccionDestino, viajeMarcado);
                        return RedirectToAction("ResumenViaje", new { viaje.Id });
                        //if (viaje == null)
                        //{
                        //    ViewBag.Mensaje("No hay vehiculos disponibles. Estamos buscando un vehiculo para usted.");
                        //    return RedirectToAction("SolicitarViaje");
                        //}
                        //else
                        //{
                        //    return RedirectToAction("ResumenViaje", new { viaje.Id });
                        //}
                    }
                    return RedirectToAction("SolicitarViaje");
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
                return RedirectToAction("SolicitarViaje");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Vuelva a intentarlo mas tarde");
                return RedirectToAction("SolicitarViaje");
            }

        }

        [HttpPost]
        [Route("AgregarItem")]
        public async Task<IActionResult> AgregarItem(ViewModelViaje model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioCliente(token))
                {
                    string idCliente = _session.GetString("UserId");
                    model.Item.Tipo = model.TipoItem;
                    await _controladoraViajes.agregarItem(idCliente, model.Item);
                    return RedirectToAction("SolicitarViaje");
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
                return RedirectToAction("SolicitarViaje");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                return RedirectToAction("SolicitarViaje");
            }
        }

        [HttpGet]
        [Route("EditarItem")]
        public async Task<IActionResult> EditarItem(string itemId)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioCliente(token))
                {
                    string idCliente = _session.GetString("UserId");
                    ViewModelViaje model = new ViewModelViaje();
                    var item = await _controladoraViajes.itemParaEditar(idCliente, itemId);
                    model.Item = item;
                    return View(model);
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
                return RedirectToAction("SolicitarViaje");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                return RedirectToAction("SolicitarViaje");
            }

        }

        [HttpPost]
        public async Task<IActionResult> EditarItem(ViewModelViaje model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioCliente(token))
                {
                    string idCliente = _session.GetString("UserId");
                    await _controladoraViajes.editarItem(idCliente, model.Item);
                    return RedirectToAction("SolicitarViaje");
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
                return RedirectToAction("SolicitarViaje");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                return RedirectToAction("SolicitarViaje");
            }

        }

        [HttpGet]
        [Route("ResumenViaje")]
        public async Task<IActionResult> ResumenViaje(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioCliente(token))
                {
                    Viaje enCurso = await _controladoraViajes.getViaje(id);
                    ViewModelViaje resumen = new ViewModelViaje();
                    if (enCurso != null)
                    {
                        if (enCurso.DireccionDestino != "")
                        {
                            resumen.PrecioEstimado = _controladoraViajes.calcularPrecio(enCurso.DuracionEstimadaTotal, enCurso.Vehiculo.Tarifa);
                            resumen.HoraEstimadaLlegadaHastaCliente = string.Format("{0:hh\\:mm}", enCurso.HoraInicio + enCurso.DuracionEstimadaHastaCliente);
                            resumen.HoraEstimadaFinalizacionViaje = string.Format("{0:hh\\:mm}", enCurso.HoraInicio + enCurso.DuracionEstimadaTotal);
                        }
                        resumen.Viaje = enCurso;
                        resumen.LatitudOrigen = _controladoraViajes.obtenerUbicacionCliente(enCurso.Cliente.Id.ToString()).Latitud;
                        resumen.LongitudOrigen = _controladoraViajes.obtenerUbicacionCliente(enCurso.Cliente.Id.ToString()).Longitud;
                        resumen.HoraInicio = string.Format("{0:hh\\:mm}", enCurso.HoraInicio);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, intente nuevamente mas tarde");
                        return RedirectToAction("SolicitarViaje");
                    }
                    return View(resumen);
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
                return RedirectToAction("SolicitarViaje");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                return RedirectToAction("SolicitarViaje");
            }

        }

        [HttpPost]
        [Route("FinalizarViaje")]
        public async Task FinalizarViaje(ViewModelViaje model)
        {
            await _controladoraViajes.finalizarViaje(model.Viaje);
            //habria que actualizar las vistas de resumen viaje con un observer por ej. o signalr
        }

        [HttpGet]
        public IActionResult MisViajes()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioCliente(token))
                {
                    var idCliente = _session.GetString("UserId");
                    return View(_controladoraViajes.viajesCliente(idCliente));
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
                return RedirectToAction("SolicitarViaje");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                return RedirectToAction("SolicitarViaje");
            }

        }

        #endregion
    }
}