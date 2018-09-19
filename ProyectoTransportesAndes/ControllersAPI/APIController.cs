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

namespace ProyectoTransportesAndes.ControllersAPI
{
    [Produces("application/json")]
    [Route("api")]
    public class APIController : Controller
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
        public APIController(IOptions<AppSettingsMongo> settings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _configuration = configuration;
            _httpContext = httpContextAccessor;
            _settings = settings;
            DBRepositoryMongo<Usuario>.Iniciar(_settings);
            DBRepositoryMongo<Cliente>.Iniciar(_settings);
            DBRepositoryMongo<Chofer>.Iniciar(_settings);
            _controladoraViajes = ControladoraViajes.getInstancia(_settings);
            _controladoraVehiculos = ControladoraVehiculos.getInstance(_settings);
            _controladoraUsuarios = ControladoraUsuarios.getInstance(_settings);
        }
        #endregion

        #region API's
 
        /// <summary>
        /// si se puede loguear devuelve el usuario en formato json
        /// crea el token para ese usuario
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="pass"></param>
        /// <returns>usuario</returns>
        [HttpGet]
        [Route("LoginAPP")]
        public async Task<JsonResult> LoginAPP(string usuario, string pass)
        {
            Usuario user = null;
            user = await _controladoraUsuarios.Login(usuario, pass);
            if (user != null)
            {
                _session.SetString("Token", Seguridad.BuildToken(user));
                _session.SetString("User", usuario);
                return Json(user);
            }
            return Json(user);
        }

        /// <summary>
        /// si se puede loguear devuelve el chofer en formato json
        /// crea el token para ese chofer 
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="pass"></param>
        /// <returns>chofer</returns>
        [HttpGet]
        [Route("LoginAPPChofer")]
        public async Task<JsonResult> LoginAPPChofer(string usuario, string pass)
        {
            Chofer chofer = null;
            chofer = await _controladoraUsuarios.LoginChofer(usuario, pass);
            if (chofer != null)
            {
                _session.SetString("Token", Seguridad.BuildToken(chofer));
                _session.SetString("User", usuario);
                return Json(chofer);
            }
            return Json(chofer);
        }

        /// <summary>
        /// retorna un cliente en formato json si lo puede registrar, si no devuelve un null
        /// </summary>
        /// <param name="cliente"></param>
        /// <returns>cliente</returns>
        [HttpPost]
        [Route("RegistroCliente")]
        public async Task<JsonResult> RegistroCliente([FromBody]Cliente cliente)
        {
            Cliente cli = await DBRepositoryMongo<Cliente>.GetUsuario(cliente.User, "Clientes");
            Chofer chofer = await DBRepositoryMongo<Chofer>.GetUsuario(cliente.User, "Choferes");
            Usuario usu = await DBRepositoryMongo<Usuario>.GetUsuario(cliente.User, "Usuarios");

            if (cli == null && chofer == null && usu == null)
            {
                Cliente nuevo = cliente;
                await DBRepositoryMongo<Cliente>.Create(nuevo, "Clientes");
                return Json(nuevo);
            }
            else
            {
                return Json(null);
            }
        }

        /// <summary>
        /// retorna un chofer en formato json si lo puede registrar, si no devuleve null
        /// </summary>
        /// <param name="nuevo"></param>
        /// <returns>chofer</returns>
        [HttpPost]
        [Route("RegistroChofer")]
        public async Task<JsonResult> RegistroChofer([FromBody]Chofer nuevo)
        {
            Usuario usu = await DBRepositoryMongo<Usuario>.GetUsuario(nuevo.User, "Usuarios");
            Usuario cliente = await DBRepositoryMongo<Cliente>.GetUsuario(nuevo.User, "Clientes");
            Usuario chofer = await DBRepositoryMongo<Chofer>.GetUsuario(nuevo.User, "Choferes");

            if (cliente == null && usu == null && chofer == null)
            {
                await DBRepositoryMongo<Chofer>.Create(nuevo, "Choferes");
                return Json(nuevo);
            }
            else
            {
                return Json(null);
            }
        }

        /// <summary>
        /// GUARDA Y DEVUELVE LA POSICION SATELITAL DEL VEHICULO DEL CHOFER PASADA
        /// </summary>
        /// <param name="idChofer"></param>
        /// <param name="latitud"></param>
        /// <param name="longitud"></param>
        /// <returns>POSICION SATELITAL</returns>
        [HttpGet]
        [Route("GuardarCoordenadasVehiculo")]
        public async Task<JsonResult> GuardarCoordenadasVehiculo(string idChofer, string latitud, string longitud)
        {

            ObjectId choferId = Utilidades.deserealizarJsonToObjectId(idChofer);
            Vehiculo vehiculo = null;
            if (!string.IsNullOrEmpty(choferId.ToString()))
            {
                vehiculo = await _controladoraVehiculos.getVehiculoChofer(choferId.ToString());
            }
            if (vehiculo != null && !string.IsNullOrEmpty(latitud) && !string.IsNullOrEmpty(longitud))
            {
                return Json(_controladoraVehiculos.guardarUbicacionVehiculo(vehiculo.Id.ToString(), latitud, longitud));
            }
            return Json(null);
        }

        /// <summary>
        /// ESTE SE UTILIZA PARA GUARDAR LAS COORDENADAS DEL CLIENTE DESDE LA VERSION WEB
        /// NO SE LE SOLICITA EL ID, YA QUE SE MANEJA DENTRO DE LA VARIABLE SESSION
        /// </summary>
        /// <param name="latitud"></param>
        /// <param name="longitud"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CoordenadasClienteWeb")]
        public JsonResult CoordenadasClienteWeb(string latitud, string longitud)
        {

            string idCliente = _session.GetString("UserId");
            if (idCliente != null)
            {
                return Json(_controladoraUsuarios.guardarUbicacionCliente(idCliente, latitud, longitud));
            }

            return Json(null);

        }

        /// <summary>
        ///GUARDA Y DEVUELVE LA POSICION SATELITAL DEL CLIENTE PASADA
        /// </summary>
        /// <param name="idCliente"></param>
        /// <param name="latitud"></param>
        /// <param name="longitud"></param>
        /// <returns>POSICION SATELITAL</returns>
        [HttpGet]
        [Route("CoordenadasCliente")]
        public JsonResult CoordenadasCliente(string idCliente, string latitud, string longitud)
        {
            ObjectId clienteId = Utilidades.deserealizarJsonToObjectId(idCliente);
            if (!string.IsNullOrEmpty(latitud) && !string.IsNullOrEmpty(longitud))
            {
                return Json(_controladoraUsuarios.guardarUbicacionCliente(clienteId.ToString(), latitud, longitud));
            }
            return Json(null);
        }

        /// <summary>
        /// EL CHOFER DA POR FINALIZADO EL VIAJE Y SE DEVUELVE EL VIAJE FINALIZADO SI SE PUDO FINALIZAR 
        /// Y DEVUELVE EL VIAJE SIN FINALIZAR SI NO SE PUDO
        /// </summary>
        /// <param name="idViaje"></param>
        /// <returns>VIAJE</returns>
        [HttpGet]
        [Route("FinalizarViaje")]
        public async Task<JsonResult> FinalizarViaje(string idViaje)
        {
            ObjectId viajeId = Utilidades.deserealizarJsonToObjectId(idViaje);
            Viaje viaje = await _controladoraViajes.finalizarViaje(viajeId.ToString());
            //viaje = Utilidades.desencriptarViaje(viaje);
            return Json(viaje);
        }

        /// <summary>
        /// LO UTILIZAMOS PARA REALIZAR PRUEBAS
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("VehiculoPrueba")]
        public JsonResult VehiculoPrueba()
        {
            return Json(_controladoraVehiculos.getVehiculoMemoria("5b60ad9ab73c94313c6c7552"));
        }

        /// <summary>
        /// RETORNA LA ULTIMA POSICION SATELITAL DEL VEHICULO SOLICITADO
        /// </summary>
        /// <param name="idVehiculo"></param>
        /// <returns>POSICION SATELITAL</returns>
        [HttpGet]
        [Route("UbicacionVehiculo")]
        public JsonResult UbicacionVehiculo(string idVehiculo)
        {
            ObjectId vehiculoId = Utilidades.deserealizarJsonToObjectId(idVehiculo);
            return Json((PosicionSatelital)_controladoraVehiculos.UbicacionVehiculos[vehiculoId.ToString()]);
        }

        /// <summary>
        /// RECIBE UN VIAJE PRECARGADO Y SE LE TERMINAN DE CARGAR DATOS COMO LOS COSTOS ESTIMADOS,
        /// LAS DEMORAS ESTIMADAS Y EL VEHICULO PARA VER SI EL CLIENTE ACEPTA
        /// </summary>
        /// <param name="viaje"></param>
        /// <param name="idCliente"></param>
        /// <returns>VIAJE</returns>
        [HttpPost]
        [Route("SolicitudServicio")]
        public async Task<JsonResult> SolicitudServicio([FromBody]Viaje viaje, string idCliente)
        {
            ObjectId clienteId = Utilidades.deserealizarJsonToObjectId(idCliente);
            viaje.Cliente.Id = clienteId;
            Viaje salida = await _controladoraViajes.solicitarViaje(viaje, TipoVehiculo.Otros);
            if (!salida.Vehiculo.Id.ToString().Equals("000000000000000000000000"))
            {
                //salida = Utilidades.desencriptarViaje(salida);
            }
            return Json(salida);
        }

        /// <summary>
        /// RECIBE EL VIAJE Y LO TOMA COMO MUDANZA Y LO DEVUELVE CON UN VEHICULO ACORDE
        /// </summary>
        /// <param name="viaje"></param>
        /// <returns>VIAJE</returns>
        [HttpPost]
        [Route("SolicitudMudanza")]
        public async Task<JsonResult> SolicitudMudanza([FromBody]Viaje viaje)
        {
            Viaje salida = await _controladoraViajes.solicitarViaje(viaje, TipoVehiculo.CamionMudanza);
            return Json(salida);
        }

        /// <summary>
        /// SE RECIBE UN PRESUPUESTO PARA COTIZAR
        /// </summary>
        /// <param name="presupuesto"></param>
        /// <returns>PREUSPUESTO</returns>
        [HttpGet]
        [Route("Presupuesto")]
        public async Task<JsonResult> Presupuesto(Presupuesto presupuesto)
        {
            await _controladoraViajes.presupuestoNuevo(presupuesto);
            return Json(presupuesto);
        }

        /// <summary>
        /// EL CLIENTE CONSULTA CADA CIERTO TIEMPO SI SU VIAJE ESTA FINALIZADO
        /// </summary>
        /// <param name="idViaje"></param>
        /// <returns>VIAJE</returns>
        [HttpGet]
        [Route("ConsultaServicioFinalizado")]
        public async Task<JsonResult> ConsultaServicioFinalizado(string idViaje)
        {
            Viaje viaje = await _controladoraViajes.getViaje(idViaje);
            if (viaje.Estado == EstadoViaje.Finalizado)
            {
                return Json(viaje);
            }
            else
            {
                return Json(new { Success = false });
            }
        }

        /// <summary>
        /// TOMA EL ENUMERADO TIPO ITEMS Y LO PASA
        /// </summary>
        /// <returns>LISTA DE TIPO ITEMS</returns>
        [HttpGet]
        [Route("TiposItems")]
        public JsonResult TiposItems()
        {
            List<string> salida = new List<string>();


            var enumValues = Enum.GetNames(typeof(TipoItem));

            foreach (var enumValue in enumValues)
            {
                salida.Add(enumValue);
            }
            return Json(salida);
        }

        /// <summary>
        ///0 si la cancelacion se realiza antes de la confirmacion
        ///100 si se realiza luego de la confirmacion
        ///-1 si se realiza luego que el chofer llego al origen. no se puede cancelar alli
        ///SE UTILIZA PARA QUE EL CLIENTE TOME LA DECISION DE QUE HACER
        /// </summary>
        /// <param name="idViaje"></param>
        /// <returns>COSTO DE CANCELACION</returns>
        [HttpGet]
        [Route("CancelarViaje")]
        public async Task<JsonResult> CancelarViaje(string idViaje)
        {
            ObjectId viajeId = Utilidades.deserealizarJsonToObjectId(idViaje);
            double salida = await _controladoraViajes.cancelarViaje(viajeId.ToString());
            return Json(salida);
        }

        /// <summary>
        /// EL VIAJE SOLICITADO SE CANCELA Y SE LE APLICAN CARGOS SEGUN CORRESPONDA
        /// </summary>
        /// <param name="idViaje"></param>
        /// <param name="costo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ConfirmarCancelacion")]
        public async Task ConfirmarCancelacion(string idViaje, string costo)
        {
            double valor = 0;
            double.TryParse(costo, out valor);
            ObjectId viajeId = Utilidades.deserealizarJsonToObjectId(idViaje);
            await _controladoraViajes.confirmarCancelacion(viajeId.ToString(), valor);
        }

        /// <summary>
        /// AL VIAJE SOLICITADO SE LO CONFIRMA Y SE COMIENZA EL PROCESO.
        /// SE LO MARCA COMO CONFIRMADO Y QUEDA A LA ESPERA DE QUE EL CHOFER LO INICIE
        /// </summary>
        /// <param name="idViaje"></param>
        /// <returns>VIAJE SOLICITADO</returns>
        [HttpGet]
        [Route("ConfirmarViaje")]
        public async Task<JsonResult> ConfirmarViaje(string idViaje)
        {
            ObjectId viajeId = Utilidades.deserealizarJsonToObjectId(idViaje);
            Viaje salida = await _controladoraViajes.confirmarViaje(viajeId.ToString());
            salida = Utilidades.desencriptarViaje(salida);
            return Json(salida);
        }

        /// <summary>
        /// AL VIAJE SOLICITADO SE LE CAMBIA EL ESTADO A "ENCURSO" Y SE TOMA COMO INICIALIZADO
        /// </summary>
        /// <param name="idViaje"></param>
        /// <returns>VIAJE SOLICITADO</returns>
        [HttpGet]
        [Route("Levantar")]
        public async Task<JsonResult> Levantar(string idViaje)
        {
            ObjectId viajeId = Utilidades.deserealizarJsonToObjectId(idViaje);
            Viaje salida = await _controladoraViajes.levanteViaje(viajeId.ToString());
            //salida = Utilidades.desencriptarViaje(salida);
            return Json(salida);
        }

        /// <summary>
        /// HISTORICO DE VIAJES DE UN CHOFER, PUEDE SER DEL MES CORRIENTE O GENERAL
        /// </summary>
        /// <param name="idChofer"></param>
        /// <returns>LISTA VIAJES CHOFER</returns>
        [HttpGet]
        [Route("ViajesChofer")]
        public async Task<JsonResult> ViajesChofer(string idChofer, string mes)
        {
            ObjectId choferId = Utilidades.deserealizarJsonToObjectId(idChofer);
            var viajes = await _controladoraViajes.getViajesChofer(choferId.ToString());
            if (mes.Equals("mes"))
            {
                viajes.Where(v => v.Fecha.Month == DateTime.Now.Month).ToList();
            }
            return Json(viajes);
        }

        /// <summary>
        /// VIAJES QUE NO ESTEN FINALIZADOS
        /// </summary>
        /// <param name="idChofer"></param>
        /// <returns>LISTA VIAJES ACTIVOS CHOFER</returns>
        [HttpGet]
        [Route("ViajesActivosChofer")]
        public async Task<JsonResult> ViajesActivosChofer(string idChofer)
        {
            ObjectId choferId = Utilidades.deserealizarJsonToObjectId(idChofer);
            List<Viaje> viajes = await _controladoraViajes.getViajesActivosChofer(choferId.ToString());
            return Json(viajes);
        }

        /// <summary>
        /// HISTORICO DE VIAJES DE UN CLIENTE, PUEDE SER DE EL MES CORRIENTE O GENERAL
        /// </summary>
        /// <param name="idCliente"></param>
        /// <returns>LISTA VIAJES CLIENTE</returns>
        [HttpGet]
        [Route("ViajesCliente")]
        public async Task<JsonResult> ViajesCliente(string idCliente, string mes)
        {
            ObjectId clienteId = Utilidades.deserealizarJsonToObjectId(idCliente);
            var viajes = await _controladoraViajes.getViajesCliente(clienteId.ToString());
            if (mes.Equals("mes"))
            {
                viajes.Where(v => v.Fecha.Month == DateTime.Now.Month).ToList();
            }
            return Json(viajes);
        }

        #endregion
    }
}