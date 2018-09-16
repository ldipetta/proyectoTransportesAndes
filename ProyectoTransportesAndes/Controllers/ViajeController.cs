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

        /// <summary>
        /// Get para el listado de viajes online. Permite realizar filtros.
        /// </summary>
        /// <returns>Retorna la vista con el view model cargado</returns>
        [HttpGet]
        [Route("Listado")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Listado()
        {
            ViewModelViajeFiltro model = new ViewModelViajeFiltro(_settings);
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    model.Viajes = await _controladoraViajes.getViajes();
                    double totalViajes = 0;
                    foreach (Viaje v in model.Viajes)
                    {
                        if(v.Estado.Equals(EstadoViaje.Finalizado) || v.Estado.Equals(EstadoViaje.Cancelado))
                        {
                            totalViajes += v.CostoFinal;
                        }
                    }
                    model.TotalViajes = totalViajes;
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
                return RedirectToAction("Index","Home");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado, intente de nuevo mas tarde";
                return RedirectToAction("Index","Home");
            }
        }

        /// <summary>
        /// Post que recibe los filtros deseados en el view model.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Retorna una lista con los viajes online filtrados</returns>
        [HttpPost]
        [Route("Listado")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Listado(ViewModelViajeFiltro model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    List<Viaje> viajes = null;
                    if (model.TipoSeleccionado.Equals("1"))
                    {
                        var aux = await _controladoraViajes.getViajes();
                        viajes = aux.ToList();
                    }
                    if (model.TipoSeleccionado.Equals("2"))
                    {
                        var aux = await _controladoraViajes.getViajesOnline();
                        viajes = aux.ToList();
                    }
                    if (model.TipoSeleccionado.Equals("3"))
                    {
                        var aux = await _controladoraViajes.getViajesDirectos();
                        viajes = aux.ToList();
                    }
                    if (!model.IdCliente.Equals("000000000000000000000000"))
                    {
                        viajes = viajes.Where(v => v.Cliente.Id.ToString().Equals(model.IdCliente)).ToList();
                    }
                    if (!model.IdVehiculo.Equals("000000000000000000000000"))
                    {
                        viajes = viajes.Where(v => v.Vehiculo.Id.ToString().Equals(model.IdVehiculo)).ToList();
                    }
                    if (!model.EstadoViaje.Equals(EstadoViaje.Estado))
                    {
                        viajes = viajes.Where(v => v.Estado.ToString().Equals(model.EstadoViaje.ToString())).ToList();
                    }
                    if (model.Desde != null)
                    {
                        viajes = viajes.Where(v => v.Fecha.Date >= model.Desde).ToList();
                    }
                    if (model.Hasta != null)
                    {
                        viajes = viajes.Where(v => v.Fecha.Date <= model.Hasta).ToList();
                    }
                    double totalViajes = 0;
                    foreach(Viaje v in viajes)
                    {
                        totalViajes += v.CostoFinal;
                    }
                    model.TotalViajes = totalViajes;
                    model.Viajes = viajes;
                    return View(model);
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
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado, intente de nuevo mas tarde");
                return View(model);
            }
        }

        /// <summary>
        /// se utiliza para que desde el backend se puedan ingresar viajes de forma 
        /// directa sin que los solicite el cliente a traves de la app
        /// </summary>
        /// <returns>vista para ingresar un nuevo viaje directo</returns>
        [HttpGet]
        [Route("Nuevo")]
        [AutoValidateAntiforgeryToken]
        public IActionResult Nuevo()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    ViewModelViajeDirecto model = new ViewModelViajeDirecto(_settings);
                    model.Fecha = DateTime.Now.Date;
                    model.FechaParaMostrar = model.Fecha.ToShortDateString();
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
                return RedirectToAction("Listado");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Listado");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns>ingresa el viaje directo segun el modelo</returns>
        [HttpPost]
        [Route("Nuevo")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Nuevo(ViewModelViajeDirecto model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    await _controladoraViajes.nuevoViaje(model.VehiculoId, model.ClienteId, model.Direccion, model.Fecha, model.HoraInicio, model.Comentarios);
                    return RedirectToAction("Listado");
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
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return View(model);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>vista para modificar el viaje directo</returns>
        [HttpGet]
        [Route("Edit")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
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
                TempData["Error"] = msg.Message;
                return RedirectToAction("Listado");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Listado");
            }
        }

        /// <summary>
        /// modifica el viaje directo segun modelo
        /// </summary>
        /// <param name="id"></param>
        /// <param name="viaje"></param>
        /// <returns>vista index</returns>
        [HttpPost]
        [Route("Edit")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(string id, Viaje viaje)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    if (ModelState.IsValid)
                    {
                        await _controladoraViajes.modificarViaje(id, viaje);
                        return RedirectToAction("Listado");
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
                TempData["Error"] = msg.Message;
                return RedirectToAction("Listado");
            }
            catch (Exception)
            {
                TempData["Error"]= "Ha ocurrido un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Listado");
            }
        }

        /// <summary>
        /// carga los datos del viaje seleccionado para elimiar en una vista
        /// </summary>
        /// <param name="id"></param>
        /// <returns>vista delete</returns>
        [HttpPost]
        [Route("Delete")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    Viaje viaje = await _controladoraViajes.getViaje(id);
                    return View(viaje);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Listado");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Listado");
            }
        }

        /// <summary>
        /// elimina de la bd el viaje seleccionado
        /// </summary>
        /// <param name="id"></param>
        /// <param name="viaje"></param>
        /// <returns>vista listado</returns>
        [HttpPost]
        [Route("Delete")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Delete(string id, Viaje viaje)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrador(token))
                {
                    await _controladoraViajes.eliminarViaje(id, viaje);
                    return RedirectToAction("Listado");
                }
                else
                {
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Listado");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Listado");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>vista para ingresar un viaje</returns>
        [HttpGet]
        [Route("SolicitarViaje")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Servicio()
        {
            ViewModelViaje model = new ViewModelViaje();
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioCliente(token))
                {
                    string cliente = _session.GetString("UserId");
                    if (!cliente.Equals(""))
                    {
                        var viajePendienteCliente = await _controladoraViajes.viajePendienteCliente(cliente);
                        var vehiculos = await  _controladoraVehiculos.vehiculosConPosicion();
                        model.Viaje = viajePendienteCliente;
                        if (model.Viaje != null)
                        {
                            model.IdViaje = viajePendienteCliente.Id.ToString();
                            model.ViajeCompartido = viajePendienteCliente.Compartido;
                        }
                        else
                        {
                            ViewData["Mensaje"] = "No tiene items ingresados";
                        }
                        model.Vehiculos = vehiculos;
                        if (model.Viaje == null)
                        {
                            model.Viaje = new Viaje();
                            model.Viaje.DireccionDestino = "";
                            ViewData["Mensaje"] = "No tiene items ingresados";
                            model.Viaje.Items = new List<Item>();
                            model.Viaje.DireccionDestino = "";
                            model.Viaje.Destino = new PosicionSatelital();
                            model.Viaje.Origen = new PosicionSatelital();
                            model.Viaje.Compartido = false;
                        }
                        PosicionSatelital ubicacionCliente = _controladoraUsuarios.obtenerUbicacionCliente(cliente);
                        if (string.IsNullOrEmpty(model.Viaje.DireccionOrigen))
                        {
                            model.Viaje.DireccionOrigen = await _controladoraVehiculos.convertirCoordenadasEnDireccion(ubicacionCliente.Latitud, ubicacionCliente.Longitud);

                        }
                        return View(model);
                    }
                    return View(model);
                }
                else
                {
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error desconocido, vuelva a intentarlo mas tarde";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// ingresa un item a un viaje pendiente o inicia el proceso de un viaje en el sistema
        /// </summary>
        /// <param name="model"></param>
        /// <returns>vista solicitar si se agrega un item o vista mis viajes si se solicita el viaje</returns>
        [HttpPost]
        [Route("SolicitarViaje")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Servicio(ViewModelViaje model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioCliente(token))
                {

                    string idCliente = _session.GetString("UserId");
                    var item = Request.Form["item"];
                    var solicitar = Request.Form["solicitar"];
                    Cliente cliente = await _controladoraUsuarios.getCliente(idCliente);
                    if (!string.IsNullOrEmpty(item))
                    {
                        if (!model.Item.Alto.HasValue || !model.Item.Ancho.HasValue || !model.Item.Ancho.HasValue || !model.Item.Peso.HasValue)
                        {
                            TempData["Error"] = "El alto, ancho, profundidad y peso del item no pueden ser vacios";
                            return RedirectToAction("Servicio");
                        }
                        if(string.IsNullOrEmpty(model.Item.DireccionOrigen) || string.IsNullOrEmpty(model.Item.DireccionDestino))
                        {
                            TempData["Error"] = "Las direcciones origen y destino del item no pueden ser vacias";
                            return RedirectToAction("Servicio");
                        }
                        else
                        {
                            Utilidades.validarDireccion(model.Item.DireccionOrigen);
                            Utilidades.validarDireccion(model.Item.DireccionDestino);
                        }
                        model.Item.Tipo = model.TipoItem;
                        model.Viaje.Cliente = cliente;
                        model.Viaje.Compartido = model.ViajeCompartido;
                        await _controladoraViajes.agregarItem(model.Viaje, model.Item);
                        return RedirectToAction("Servicio");
                    }
                    if (!string.IsNullOrEmpty(solicitar))
                    {
                        if (model.IdViaje != null) // si viene con id es que ya se le agregaron items
                        {
                            Viaje viaje = await _controladoraViajes.getViajePendiente(model.IdViaje);
                            if (model.ViajeCompartido)
                            {
                                if(viaje.Items==null || viaje.Items.Count == 0)
                                {
                                    ModelState.AddModelError(string.Empty, "Debe ingresar por lo menos un item.");
                                    return View(model);
                                }
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
                        { // viaje sin items
                            if (string.IsNullOrEmpty(model.Viaje.DireccionOrigen))
                            {
                                TempData["Error"]= "Debe ingresar al menos la dirección de origen, o agregar algún item";
                                return RedirectToAction("Servicio");
                            }
                            Viaje nuevo = new Viaje();
                            if (string.IsNullOrEmpty(model.Viaje.DireccionDestino))
                            {
                                nuevo.DireccionDestino = model.Viaje.DireccionOrigen;
                            }
                            else
                            {
                                nuevo.DireccionDestino = model.Viaje.DireccionDestino;
                            }
                            nuevo.DireccionOrigen = model.Viaje.DireccionOrigen;
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
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Servicio");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado. Vuelva a intentarlo mas tarde";
                return RedirectToAction("Servicio");
            }

        }

        /// <summary>
        /// se utiliza para ver los detalles luego de la solicitud asi como tambien para que el cliente vea los detalles de sus viajes
        /// </summary>
        /// <param name="idViaje"></param>
        /// <returns>vista con detalles del viaje</returns>
        [HttpGet]
        [Route("Resumen")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Resumen(string idViaje)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioCliente(token))
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
                                if (enCurso.Compartido)
                                {
                                    resumen.HoraEstimadaFinalizacionViaje = "Dentro de las 6 horas siguientes a la solicitud";
                                }
                                else
                                {
                                    resumen.HoraEstimadaFinalizacionViaje = string.Format("{0:hh\\:mm}", enCurso.HoraInicio + enCurso.DuracionEstimadaTotal);
                                }
                            }
                            if (enCurso.Compartido)
                            {
                                resumen.HoraEstimadaLlegadaHastaCliente = "";
                                resumen.DuracionEstimadaViaje = "";
                            }
                            else
                            {
                                resumen.HoraEstimadaLlegadaHastaCliente = string.Format("{0:hh\\:mm}", enCurso.HoraInicio + enCurso.DuracionEstimadaHastaCliente) + " minutos";
                                resumen.DuracionEstimadaViaje = enCurso.DuracionEstimadaTotal.Minutes.ToString() + " minutos";

                            }
                            resumen.Viaje = enCurso;
                            resumen.LatitudOrigen = _controladoraUsuarios.obtenerUbicacionCliente(enCurso.Cliente.Id.ToString()).Latitud;
                            resumen.LongitudOrigen = _controladoraUsuarios.obtenerUbicacionCliente(enCurso.Cliente.Id.ToString()).Longitud;
                            resumen.HoraInicio = string.Format("{0:hh\\:mm}", enCurso.HoraInicio);
                            resumen.FechaParaMostrar = enCurso.Fecha.ToShortDateString();
                            resumen.IdViaje = enCurso.Id.ToString();
                            resumen.VehiculoParaMostrar = enCurso.Vehiculo.ToString();
                        }
                        else
                        {
                            TempData["Error"] = "Ha ocurrido un error inesperado, intente nuevamente mas tarde";
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
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Servicio");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde";
                return RedirectToAction("Servicio");
            }
        }

        /// <summary>
        /// se carga en la vista todos los viajes del cliente con opcion de filtros
        /// </summary>
        /// <returns>vista mis viajes</returns>
        [HttpGet]
        [Route("MisViajes")]
        [ActionName("MisViajes")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> MisViajes()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioCliente(token))
                {
                    var idCliente = _session.GetString("UserId");
                    var viajesCliente = await _controladoraViajes.getViajesCliente(idCliente);
                    ViewModelViajeFiltro model = new ViewModelViajeFiltro();
                    model.Viajes = viajesCliente.ToList();
                    model.IdCliente = idCliente;
                    return View(model);
                }
                else
                {
                    return RedirectToAction("Login","Account");
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
                return RedirectToAction("Index","Home");
            }

        }

        /// <summary>
        /// aplica los filtros deseados
        /// </summary>
        /// <param name="model"></param>
        /// <returns>vista mis viajes</returns>
        [HttpPost]
        [Route("MisViajes")]
        [ActionName("MisViajes")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> MisViajes(ViewModelViajeFiltro model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioCliente(token))
                {
                    var viajes = await _controladoraViajes.getViajesCliente(model.IdCliente);
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
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index","Cliente");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado, intente de nuevo mas tarde";
                return RedirectToAction("Index","Cliente");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns>vista resume si se solicita una mudanza y vista presupuesto si se solicito un presupuesto</returns>
        [HttpGet]
        [Route("Mudanza")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Mudanza(ViewModelViaje model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioCliente(token))
                {
                    var mudanza = Request.Form["mudanza"];
                    var presupuesto = Request.Form["presupuesto"];
                    var idCliente = _session.GetString("UserId");
                    Cliente cliente = await _controladoraUsuarios.getCliente(idCliente);
                    if (!string.IsNullOrEmpty(mudanza))
                    {
                        if (string.IsNullOrEmpty(model.Viaje.DireccionDestino))
                        {
                            model.Viaje.DireccionDestino = model.Viaje.DireccionOrigen;
                        }
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
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Servicio");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Servicio");
            }
        }

        /// <summary>
        /// confirma o cancela el viaje del modelo
        /// </summary>
        /// <param name="viaje"></param>
        /// <returns>vista mis viajes</returns>
        [Route("Confirmacion")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Confirmacion(ViewModelViaje viaje)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioCliente(token))
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
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Resumen");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Resumen");
            }
        }

        /// <summary>
        /// consulta cual es el costo de cancelar un viaje y lo devuelve en otra lista
        /// </summary>
        /// <param name="idViaje"></param>
        /// <returns>vista con la confirmacion si se quiere cancelar el viaje</returns>
        [HttpGet]
        [Route("CancelarViaje")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CancelarViaje(string idViaje)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioCliente(token))
                {
                    double salida = await _controladoraViajes.cancelarViaje(idViaje);
                    Viaje viaje = new Viaje();
                    viaje.CostoFinal = salida;
                    viaje.Id = new MongoDB.Bson.ObjectId(idViaje);
                    return View(viaje);
                }
                else
                {
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Resumen");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Resumen");
            }
        }

        /// <summary>
        /// si confirma la cancelacion se cancela el viaje y se da por finalizado y si no se confirma no se modifica nada
        /// </summary>
        /// <param name="id"></param>
        /// <param name="viaje"></param>
        /// <returns>vista mis viajes</returns>
        [HttpPost]
        [Route("CancelarViaje")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CancelarViaje(string id,Viaje viaje)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioCliente(token))
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
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Resumen");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Resumen");
            }
        }

        /// <summary>
        /// notifica la solicitud del presupuesto
        /// </summary>
        /// <returns>vista presupuesto</returns>
        [Route("Presupuesto")]
        [AutoValidateAntiforgeryToken]
        public IActionResult Presupuesto()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioCliente(token))
                {
                    return View();
                }
                else
                {
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Resumen");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Resumen");
            }
        }

        /// <summary>
        /// permite que solo un administrador modifique las tarifas
        /// </summary>
        /// <returns>vista para modificar las tarifas</returns>
        [HttpGet]
        [Route("Tarifas")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Tarifas()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrador(token))
                {
                    Tarifa ultima = await _controladoraViajes.obtenerUltimaTarifa();
                    ViewModelTarifa model = new ViewModelTarifa();
                    if (ultima != null)
                    {
                        model.UltimaActualizacion = ultima.FechaModificacion;
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
                    return RedirectToAction("Login","Äccount");
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

        /// <summary>
        /// cambia los valores de las tarifas
        /// </summary>
        /// <param name="model"></param>
        /// <returns>vista tarifa con resultado</returns>
        [HttpPost]
        [Route("Tarifas")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Tarifas(ViewModelTarifa model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrador(token))
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
                        return View(model);
                    }
                    await _controladoraVehiculos.actualizarTarifasVehiculos(nueva);
                    ModelState.AddModelError(string.Empty, "La tarifa se actualizó con exito");
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos, inicie sesión");
                    return RedirectToAction("Login","Account");
                }
            }
            catch (FormatException)
            {
                ModelState.AddModelError(string.Empty, "Debe ingresar solo numeros como valores de la tarifa");
                return View(model);
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns>vista para liquidar viajes de un chofer</returns>
        [HttpGet]
        [Route("LiquidacionViajesChofer")]
        [AutoValidateAntiforgeryToken]
        public IActionResult LiquidacionViajesChofer()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    ViewModelLiquidacion model = new ViewModelLiquidacion();
                    LiquidacionChofer liquidacion = new LiquidacionChofer();
                    model.Liquidacion = liquidacion;
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No tiene los permisos, inicie sesión");
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index","Home");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// si se selecciona liquidar hace el calculo de la liquidacion y lo devuelve para su confirmacion
        /// si se selecciona cancelar se cancela la liquidacion en curso y se vuelve a la vista para iniciar otra liquidacion
        /// si se se selecciona confirmar se confirma la liquidacion en curso y se guarda por si se quiere consultar luego y se redirige
        /// hacia la vista de las liquidaciones generales
        /// </summary>
        /// <param name="model"></param>
        /// <param name="idLiquidacionChofer"></param>
        /// <returns>vista actualizada con la posible liquidacion del chofer seleccionado</returns>
        [HttpPost]
        [Route("LiquidacionViajesChofer")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> LiquidacionViajesChofer(ViewModelLiquidacion model,string idLiquidacionChofer)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    var confirmar = Request.Form["confirmar"];
                    var cancelar = Request.Form["cancelar"];
                    var liquidar = Request.Form["liquidar"];
                    var userId = _session.GetString("UserId");

                    if (!string.IsNullOrEmpty(confirmar))
                    {
                        await _controladoraViajes.confirmarLiquidacion(model.IdLiquidacionChofer);
                        return RedirectToAction("Liquidaciones");
                    }
                    else if (!string.IsNullOrEmpty(cancelar))
                    {
                        await _controladoraViajes.cancelarLiquidacion(model.IdLiquidacionChofer);
                        return RedirectToAction("LiquidacionViajesChofer");
                    }
                    else if (!string.IsNullOrEmpty(liquidar))
                    {
                        model.Liquidacion = await _controladoraViajes.getLiquidacionChofer(model.IdChofer, userId);
                        model.Liquidacion = await _controladoraViajes.liquidar(model.Liquidacion);
                        model.Liquidar = true;
                        model.IdLiquidacionChofer = model.Liquidacion.Id.ToString();
                        return View(model);
                    }
                    else
                    {
                        model.Liquidacion = await _controladoraViajes.getLiquidacionChofer(model.IdChofer, userId);
                        return View(model);
                    }
                }
                else
                {
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Liquidaciones");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Liquidaciones");
            }
        }
    
        /// <summary>
        /// carga las liquidaciones realizadas
        /// </summary>
        /// <returns>vista liquidaciones</returns>
        [HttpGet]
        [Route("Liquidaciones")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Liquidaciones()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    List<LiquidacionChofer> liquidaciones = await _controladoraViajes.getLiquidacionesRealizadas();
                    ViewModelFiltroLiquidaciones filtro = new ViewModelFiltroLiquidaciones();
                    filtro.Liquidaciones = liquidaciones;
                    return View(filtro);
                }
                else
                {
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index","Home");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// filtra las liquidaciones segun modelo
        /// </summary>
        /// <param name="filtro"></param>
        /// <returns>vista liquidaciones</returns>
        [HttpPost]
        [Route("Liquidaciones")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Liquidaciones(ViewModelFiltroLiquidaciones filtro)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    List<LiquidacionChofer> liquidaciones = await _controladoraViajes.getLiquidacionesRealizadas();
                    if (filtro.Equals("000000000000000000000000"))
                    {
                        filtro.Liquidaciones = liquidaciones;
                    }
                    else
                    {
                        filtro.Liquidaciones = liquidaciones.Where(l => l.Chofer.Id.ToString().Equals(filtro.IdChofer)).ToList();
                    }
                    return View(filtro);
                }
                else
                {
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                ModelState.AddModelError(string.Empty, msg.Message);
                return View(filtro);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                return View(filtro);
            }
        }

        /// <summary>
        /// carga todos los presupuestos pendientes que hay en el sistema
        /// </summary>
        /// <returns>vista presupuestos</returns>
        [HttpGet]
        [Route("Presupuestos")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Presupuestos()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    List<Presupuesto> presupuestos = await _controladoraViajes.presupuestosPendientes();
                    return View(presupuestos);
                }
                else
                {
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index","Home");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// confirma el presupuesto pasado como realizado
        /// </summary>
        /// <param name="idPresupuesto"></param>
        /// <returns>vista presupuestos</returns>
        [HttpGet]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> RealizarPresupuesto(string idPresupuesto)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    await _controladoraViajes.confirmarPresupuesto(idPresupuesto);
                    return RedirectToAction("Presupuestos");
                }
                else
                {
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Presupuestos");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Presupuestos");
            }
        }

        /// <summary>
        /// en la vista se selecciona el vehiculo deseado
        /// </summary>
        /// <returns>vista estadisticasvehiculo</returns>
        [HttpGet]
        [Route("EstadisticasVehiculo")]
        [AutoValidateAntiforgeryToken]
        public IActionResult EstadisticasVehiculo()
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    ViewModelEstadisticas model = new ViewModelEstadisticas();
                    return View(model);
                }
                else
                {
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Index","Home");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado. Intente de nuevo mas tarde";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// en la vista carga las estadisticas del vehiculo seleccionado
        /// </summary>
        /// <param name="model"></param>
        /// <returns>vista estadisticasvehiculo</returns>
        [HttpPost]
        [Route("EstadisticasVehiculo")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> EstadisticasVehiculo(ViewModelEstadisticas model)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrativo(token))
                {
                    if (!string.IsNullOrEmpty(model.AñoSeleccionado))
                    {
                        model.Estadistica = await _controladoraViajes.estadisticaVehiculo(model.AñoSeleccionado, model.MesSeleccionado, model.IdVehiculo);
                        return View(model);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Debe ingresar un año");
                        return View(model);
                    }
                }
                else
                {
                    return RedirectToAction("Login","Account");
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

        /// <summary>
        /// le permite finalizar un viaje manualmente al administrador ante cualquier eventualidad
        /// </summary>
        /// <param name="model"></param>
        /// <returns>vista listado</returns>
        [HttpGet]
        [Route("FinalizarViaje")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> FinalizarViaje(string idViaje)
        {
            try
            {
                var token = _session.GetString("Token");
                if (Seguridad.validarUsuarioAdministrador(token))
                {
                    await _controladoraViajes.finalizarViaje(idViaje);
                    return RedirectToAction("Listado");
                }
                else
                {
                    return RedirectToAction("Login","Account");
                }
            }
            catch (MensajeException msg)
            {
                TempData["Error"] = msg.Message;
                return RedirectToAction("Listado");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde";
                return RedirectToAction("Listado");
            }
        }
    }
        #endregion
}