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
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.text.html.simpleparser;

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
        [Route("ViajesOnline")]
        public async Task<IActionResult> ViajesOnline()
        {
            ViewModelViajeFiltro model = new ViewModelViajeFiltro(_settings);
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    model.Viajes = await _controladoraViajes.getViajesOnline();
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
        [Route("ViajesOnline")]
        public async Task<IActionResult> ViajesOnline(ViewModelViajeFiltro model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    var viajes = await _controladoraViajes.getViajesOnline();

                    if (!model.IdCliente.Equals("000000000000000000000000"))
                    {
                        Cliente cliente = await _controladoraUsuarios.getCliente(model.IdCliente);
                        viajes.Where(v => v.Cliente.Equals(cliente));
                    }
                    if (!model.IdVehiculo.Equals("0"))
                    {
                        Vehiculo vehiculo = _controladoraVehiculos.getVehiculo(model.IdVehiculo);
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
        [Route("ViajesDirectos")]
        public async Task<IActionResult> ViajesDirectos()
        {
            ViewModelViajeFiltro model = new ViewModelViajeFiltro(_settings);
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    model.Viajes = await _controladoraViajes.getViajesDirectos();
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
        [Route("ViajesDirectos")]
        public async Task<IActionResult> ViajesDirectos(ViewModelViajeFiltro model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    var viajes = await _controladoraViajes.getViajesDirectos();

                    if (!model.IdCliente.Equals("000000000000000000000000"))
                    {
                        Cliente cliente = await _controladoraUsuarios.getCliente(model.IdCliente);
                        viajes.Where(v => v.Cliente.Equals(cliente));
                    }
                    if (!model.IdVehiculo.Equals("0"))
                    {
                        Vehiculo vehiculo = _controladoraVehiculos.getVehiculo(model.IdVehiculo);
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
                    model.Fecha = DateTime.Now.Date;
                    model.FechaParaMostrar = model.Fecha.ToShortDateString();
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
                    await _controladoraViajes.nuevoViaje(model.VehiculoId, model.ClienteId, model.Direccion, model.Fecha, model.HoraInicio, model.Comentarios);
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
        public async Task<IActionResult> Servicio()
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
                            model.Viaje.DireccionDestino = "";
                            ViewData["Mensaje"] = "No tiene items ingresados";
                            model.Viaje.Items = new List<Item>();
                            model.Viaje.DireccionDestino = "";
                            model.Viaje.Destino = new PosicionSatelital();
                        }
                        PosicionSatelital ubicacionCliente = _controladoraViajes.obtenerUbicacionCliente(cliente);
                        model.Viaje.DireccionOrigen = await _controladoraVehiculos.convertirCoordenadasEnDireccion(ubicacionCliente.Latitud, ubicacionCliente.Longitud);
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
        public async Task<IActionResult> Servicio(ViewModelViaje model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioCliente(token))
                {

                    string idCliente = _session.GetString("UserId");
                    var item = Request.Form["item"];
                    var solicitar = Request.Form["solicitar"];
                    Cliente cliente = await _controladoraUsuarios.getCliente(idCliente);
                    if (!string.IsNullOrEmpty(item))
                    {
                        if (!model.Item.Alto.HasValue || !model.Item.Ancho.HasValue || !model.Item.Ancho.HasValue || !model.Item.Peso.HasValue)
                        {
                            ModelState.AddModelError(string.Empty, "El alto, ancho, profundidad y peso del item no pueden ser vacios");
                            return RedirectToAction("Servicio");
                        }
                        model.Item.Tipo = model.TipoItem;
                        model.Viaje.Cliente = cliente;
                        await _controladoraViajes.agregarItem(model.Viaje, model.Item);
                        return RedirectToAction("Servicio");
                    }
                    if (!string.IsNullOrEmpty(solicitar))
                    {
                        if (model.IdViaje != null)
                        {
                            Viaje viaje = await _controladoraViajes.getViajePendiente(model.IdViaje);
                            viaje.DireccionOrigen = model.Viaje.DireccionOrigen;
                            viaje.DireccionDestino = model.Viaje.DireccionDestino;
                            if (model.ViajeCompartido)
                            {
                                viaje.Compartido = true;
                            }
                            else
                            {
                                viaje.Compartido = false;
                            }
                            if (viaje.Items != null && viaje.Items.Count > 0)
                            {
                                viaje = await _controladoraViajes.corroborarDireccionesItems(viaje);
                            }
                            viaje = await _controladoraViajes.solicitarViaje(viaje, TipoVehiculo.Otros);
                            if(string.IsNullOrEmpty(viaje.Vehiculo.Matricula) && string.IsNullOrEmpty(viaje.Vehiculo.Marca) && string.IsNullOrEmpty(viaje.Vehiculo.Modelo))
                            {
                                ModelState.AddModelError(string.Empty, "No hay vehículos disponibles por el momento. Intente nuevamente mas tarde.");
                                return RedirectToAction("Servicio");
                            }
                            else
                            {
                                return RedirectToAction("Resumen", new { idViaje = viaje.Id.ToString() });

                            }

                        }
                        else
                        {
                            Viaje nuevo = new Viaje();
                            nuevo.DireccionOrigen = model.Viaje.DireccionOrigen;
                            nuevo.DireccionDestino = model.Viaje.DireccionDestino;
                            nuevo.Cliente = cliente;
                            if (model.ViajeCompartido)
                            {
                                nuevo.Compartido = true;
                            }
                            else
                            {
                                nuevo.Compartido = false;
                            }
                            nuevo = await _controladoraViajes.solicitarViaje(nuevo, TipoVehiculo.Otros);
                            return RedirectToAction("Resumen", new { idViaje = nuevo.Id.ToString() });
                        }
                    }
                    return RedirectToAction("Servicio");
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
                return RedirectToAction("Servicio");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Vuelva a intentarlo mas tarde");
                return RedirectToAction("Servicio");
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
        [Route("Resumen")]
        public async Task<IActionResult> Resumen(string idViaje)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioCliente(token))
                {
                    if (!string.IsNullOrEmpty(idViaje))
                    {
                        ViewModelViaje resumen = new ViewModelViaje();
                        resumen.DetallesViaje = false;
                        Viaje enCurso = null;
                        enCurso = await _controladoraViajes.getViajePendiente(idViaje);
                        if (enCurso == null)
                        {
                            enCurso = await _controladoraViajes.getViaje(idViaje);
                            resumen.DetallesViaje = true;
                        }

                        if (enCurso != null)
                        {
                            PosicionSatelital posicionActualVehiculo = ControladoraVehiculos.getInstance(_settings).obtenerUltimaUbicacionVehiculo(enCurso.Vehiculo.Id.ToString());
                            if (!string.IsNullOrEmpty(enCurso.DireccionDestino))
                            {
                                resumen.PrecioEstimado = _controladoraViajes.calcularPrecio(enCurso.DuracionEstimadaTotal, enCurso.Vehiculo.Tarifa, enCurso.Compartido);
                                resumen.HoraEstimadaFinalizacionViaje = string.Format("{0:hh\\:mm}", enCurso.HoraInicio + enCurso.DuracionEstimadaTotal);
                            }
                            resumen.Viaje = enCurso;
                            resumen.HoraEstimadaLlegadaHastaCliente = string.Format("{0:hh\\:mm}", enCurso.HoraInicio + enCurso.DuracionEstimadaHastaCliente);
                            resumen.LatitudOrigen = _controladoraViajes.obtenerUbicacionCliente(enCurso.Cliente.Id.ToString()).Latitud;
                            resumen.LongitudOrigen = _controladoraViajes.obtenerUbicacionCliente(enCurso.Cliente.Id.ToString()).Longitud;
                            resumen.HoraInicio = string.Format("{0:hh\\:mm}", enCurso.HoraInicio);
                            resumen.FechaParaMostrar = enCurso.Fecha.ToShortDateString();
                            resumen.IdViaje = enCurso.Id.ToString();
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, intente nuevamente mas tarde");
                            return RedirectToAction("Servicio");
                        }
                        return View(resumen);
                    }
                    else
                    {
                        ViewModelViaje resumen = new ViewModelViaje();
                        return View(resumen);
                    }

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
                return RedirectToAction("Servicio");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                return RedirectToAction("Servicio");
            }

        }

        [HttpPost]
        [Route("FinalizarViaje")]
        public async Task FinalizarViaje(ViewModelViaje model)
        {
            await _controladoraViajes.finalizarViaje(model.Viaje);
            //habria que actualizar las vistas de resumen viaje con un observer por ej. o signalr
        }

        [Route("MisViajes")]
        [HttpGet]
        public async Task<IActionResult> MisViajes()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioCliente(token))
                {
                    var idCliente = _session.GetString("UserId");
                    var viajesCliente = await _controladoraViajes.viajesCliente(idCliente);
                    ViewModelViajeFiltro model = new ViewModelViajeFiltro();
                    model.Viajes = viajesCliente.ToList();
                    model.IdCliente = idCliente;
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
        [Route("MisViajes")]
        [HttpPost]
        public async Task<IActionResult> MisViajes(ViewModelViajeFiltro model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioCliente(token))
                {
                    var viajes = await _controladoraViajes.viajesCliente(model.IdCliente);
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

        [Route("Mudanza")]
        public async Task<IActionResult> Mudanza(ViewModelViaje model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioCliente(token))
                {
                    var mudanza = Request.Form["mudanza"];
                    var presupuesto = Request.Form["presupuesto"];
                    var idCliente = _session.GetString("UserId");
                    Cliente cliente = await _controladoraUsuarios.getCliente(idCliente);
                    if (!string.IsNullOrEmpty(mudanza))
                    {
                        Viaje nuevo = model.Viaje;
                        nuevo.Cliente = cliente;
                        Viaje salida = await _controladoraViajes.solicitarViaje(nuevo, TipoVehiculo.CamionMudanza);
                        return RedirectToAction("Resumen", new { idViaje = salida.Id.ToString() });
                    }
                    if (!string.IsNullOrEmpty(presupuesto))
                    {
                        Presupuesto presupuestoNuevo = new Presupuesto();
                        presupuestoNuevo.Cliente = cliente;
                        presupuestoNuevo.DireccionDestino = model.Viaje.DireccionDestino;
                        presupuestoNuevo.DireccionOrigen = model.Viaje.DireccionOrigen;
                        presupuestoNuevo.Observaciones = model.Observaciones;
                        presupuestoNuevo.Realizado = false;
                        await _controladoraViajes.presupuestoNuevo(presupuestoNuevo);
                        return RedirectToAction("Presupuesto");
                    }
                    return View();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos, inicie sesión");
                    return RedirectToAction("Login");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Servicio");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Servicio");
            }
        }

        [Route("Confirmacion")]
        public async Task<IActionResult> Confirmacion(ViewModelViaje viaje)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioCliente(token))
                {
                    var confirmar = Request.Form["confirmar"];
                    var cancelar = Request.Form["cancelar"];
                    if (!string.IsNullOrEmpty(confirmar))
                    {
                        await _controladoraViajes.confirmarViaje(viaje.IdViaje);
                    }
                    if (!string.IsNullOrEmpty(cancelar))
                    {
                        double salida = await _controladoraViajes.cancelarViaje(viaje.IdViaje);
                    }
                    return RedirectToAction("MisViajes");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos, inicie sesión");
                    return RedirectToAction("Login");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Resumen");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Resumen");
            }
        }
        [HttpGet]
        [Route("CancelarViaje")]
        public async Task<IActionResult> CancelarViaje(string idViaje)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioCliente(token))
                {
                    double salida = await _controladoraViajes.cancelarViaje(idViaje);
                    Viaje viaje = new Viaje();
                    viaje.CostoFinal = salida;
                    viaje.Id = new MongoDB.Bson.ObjectId(idViaje);
                    return View(viaje);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos, inicie sesión");
                    return RedirectToAction("Login");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Resumen");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Resumen");
            }
        }
        [HttpPost]
        [Route("CancelarViaje")]
        public async Task<IActionResult> CancelarViaje(string id,Viaje viaje)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioCliente(token))
                {
                    var confirmar = Request.Form["confirmar"];
                    var cancelar = Request.Form["cancelar"];
                    if (!string.IsNullOrEmpty(confirmar))
                    {
                        await _controladoraViajes.confirmarCancelacion(id,viaje.CostoFinal);
                        return RedirectToAction("MisViajes");
                    }
                    if (!string.IsNullOrEmpty(cancelar))
                    {
                        return RedirectToAction("MisViajes");
                    }
                    return RedirectToAction("MisViajes");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos, inicie sesión");
                    return RedirectToAction("Login");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Resumen");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Resumen");
            }
        }


        [Route("Presupuesto")]
        public IActionResult Presupuesto()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioCliente(token))
                {
                    return View();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos, inicie sesión");
                    return RedirectToAction("Login");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Resumen");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Resumen");
            }
        }

        [HttpGet]
        [Route("Tarifas")]
        public async Task<IActionResult> Tarifas()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrador(token))
                {
                    Tarifa ultima = await _controladoraViajes.obtenerUltimaTarifa();
                    ViewModelTarifa model = new ViewModelTarifa();
                    if (ultima != null)
                    {
                        model.Camion = ultima.Camion.ToString();
                        model.CamionChico = ultima.CamionChico.ToString();
                        model.Camioneta = ultima.Camioneta.ToString();
                        model.CamionGrande = ultima.CamionGrande.ToString();
                        model.CamionMudanza = ultima.CamionMudanza.ToString();
                    }
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos, inicie sesión");
                    return RedirectToAction("Login");
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
        [Route("Tarifas")]
        public async Task<IActionResult> Tarifas(ViewModelTarifa model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrador(token))
                {
                    var userId = _session.GetString("UserId");
                    int camion, camioneta, camionChico, camionGrande, camionMudanza;
                    Tarifa nueva = new Tarifa();
                    int.TryParse(model.Camion, out camion);
                    int.TryParse(model.Camioneta, out camioneta);
                    int.TryParse(model.CamionChico, out camionChico);
                    int.TryParse(model.CamionGrande, out camionGrande);
                    int.TryParse(model.CamionMudanza, out camionMudanza);
                    nueva.Camion = camion;
                    nueva.CamionChico = camionChico;
                    nueva.Camioneta = camioneta;
                    nueva.CamionMudanza = camionMudanza;
                    nueva.CamionGrande = camionGrande;
                    await _controladoraViajes.guardarTarifa(nueva, userId);
                    if (nueva.Id == null)
                    {
                        ModelState.AddModelError(string.Empty, "Ocurrió un problema inesperado. Intente de nuevo mas tarde");
                        return View();
                    }
                    await _controladoraVehiculos.actualizarTarifasVehiculos(nueva);
                    ModelState.AddModelError(string.Empty, "La tarifa se actualizó con exito");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos, inicie sesión");
                    return RedirectToAction("Login");
                }
            }
            catch (FormatException)
            {
                ModelState.AddModelError(string.Empty, "Debe ingresar solo numeros como valores de la tarifa");
                return RedirectToAction("Tarifa");
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Tarifa");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Tarifa");
            }
        }

        [HttpGet]
        public IActionResult LiquidacionViajesChofer()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    ViewModelLiquidacion model = new ViewModelLiquidacion();
                    LiquidacionChofer liquidacion = new LiquidacionChofer();
                    model.Liquidacion = liquidacion;
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos, inicie sesión");
                    return RedirectToAction("Login");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Resumen");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Resumen");
            }
        }

        [HttpPost]
        public async Task<IActionResult> LiquidacionViajesChofer(ViewModelLiquidacion model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    var confirmar = Request.Form["confirmar"];
                    var cancelar = Request.Form["cancelar"];
                    var liquidar = Request.Form["liquidar"];
                    var userId = _session.GetString("UserId");
                    if (!string.IsNullOrEmpty(confirmar))
                    {
                        await _controladoraViajes.confirmarLiquidacion(model.Liquidacion);
                        using (MemoryStream stream = new System.IO.MemoryStream())
                        {
                            StringReader sr = new StringReader(model.Documento);
                            Document pdf = new Document(PageSize.A4, 10f, 10f, 100f, 0f);
                            PdfWriter writer = PdfWriter.GetInstance(pdf, stream);
                            pdf.Open();
                            XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdf, sr);
                            pdf.Close();
                            return File(stream.ToArray(), "application/pdf", "Grid.pdf");
                        }
                    }
                    else if (!string.IsNullOrEmpty(cancelar))
                    {
                        await _controladoraViajes.cancelarLiquidacion(model.IdLiquidacionChofer);
                        return RedirectToAction("LiquidacionViajesChofer");
                    }
                    else if (!string.IsNullOrEmpty(liquidar))
                    {
                        model.Liquidacion = _controladoraViajes.liquidar(model.Liquidacion);
                        return View(model);
                    }
                    else
                    {
                        model.Liquidacion = await _controladoraViajes.viajesParaLiquidarChofer(model.IdChofer, userId);
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos, inicie sesión");
                    return RedirectToAction("Login");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Resumen");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Resumen");
            }

        }
    
        [HttpGet]
        public async Task<IActionResult> Liquidaciones()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    List<LiquidacionChofer> liquidaciones = await _controladoraViajes.liquidacionesRealizadas(); ;
                    return View(liquidaciones);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos, inicie sesión");
                    return RedirectToAction("Login");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Resumen");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Resumen");
            }
        }

        //lista con los presupuestos pendientes que estan para realizar
        [HttpGet]
        public async Task<IActionResult> Presupuestos()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    List<Presupuesto> presupuestos = await _controladoraViajes.presupuestosPendientes();
                    return View(presupuestos);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos, inicie sesión");
                    return RedirectToAction("Login");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Resumen");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Resumen");
            }
        }

        [HttpGet]
        public async Task<IActionResult> RealizarPresupuesto(string idPresupuesto)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    await _controladoraViajes.confirmarPresupuesto(idPresupuesto);
                    return RedirectToAction("Presupuestos");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos, inicie sesión");
                    return RedirectToAction("Login");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Resumen");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Resumen");
            }
        }


        [HttpGet]
        [Route("EstadisticasVehiculo")]
        public IActionResult EstadisticasVehiculo()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    ViewModelEstadisticas model = new ViewModelEstadisticas();
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos, inicie sesión");
                    return RedirectToAction("Login");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Resumen");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Resumen");
            }
        }

        [HttpPost]
        [Route("EstadisticasVehiculo")]
        public async Task<IActionResult> EstadisticasVehiculo(ViewModelEstadisticas model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Usuario.validarUsuarioAdministrativo(token))
                {
                    if (!string.IsNullOrEmpty(model.AñoSeleccionado))
                    {
                        model.Estadistica = await _controladoraViajes.estadisticaVehiculo(model.AñoSeleccionado, model.MesSeleccionado, model.IdVehiculo);
                        return View(model);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Debe ingresar un año");
                        return RedirectToAction("EstadisticasVehiculo");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos, inicie sesión");
                    return RedirectToAction("Login");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return RedirectToAction("Resumen");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return RedirectToAction("Resumen");
            }
        }
    }
        #endregion
}