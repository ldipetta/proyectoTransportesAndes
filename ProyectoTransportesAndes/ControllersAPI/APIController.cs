using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Exceptions;
using ProyectoTransportesAndes.Models;
using ProyectoTransportesAndes.Persistencia;
using Newtonsoft.Json;

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
        //Servicio para loguearse desde las Apps móviles, devuelve un json con datos del usuario para
        //ser utilizados por las Apps
        
            //Que pasa con el token? se pude levantar la variable session en xamarin?
        [HttpGet]
        [Route("LoginAPP")]
        public async Task<JsonResult> LoginAPP(string usuario, string pass)
        {
            Usuario user = null;
            try
            {
                if (ModelState.IsValid)
                {
                    user = await _controladoraUsuarios.Login(usuario, pass);
                    if (user == null)
                    {
                        _session.SetString("Token", Usuario.BuildToken(user));
                        _session.SetString("User", usuario);
                        return Json(user);
                    }
                    return Json(user);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error al iniciar sesión");
                    return Json(user);
                }
            }catch(MensajeException msg)
            {
                throw msg;
            }
           
        }

        [HttpGet]
        [Route("LoginAPPChofer")]
        public async Task<JsonResult> LoginAPPChofer(string usuario, string pass)
        {
            Chofer chofer = null;
            try
            {
                if (ModelState.IsValid)
                {
                    chofer = await _controladoraUsuarios.LoginChofer(usuario, pass);
                    if (chofer == null)
                    {
                        _session.SetString("Token", Usuario.BuildToken(chofer));
                        _session.SetString("User", usuario);
                        return Json(chofer);
                    }
                    return Json(chofer);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error al iniciar sesión");
                    return Json(chofer);
                }
            }
            catch (MensajeException msg)
            {
                throw msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
        
        //es el metodo que la app chofer llama en el hilo para actualizar su posicion
        [HttpGet]
        [Route("GuardarCoordenadasVehiculo")]
        public async Task<JsonResult> GuardarCoordenadasVehiculo(string idChofer, string latitud, string longitud)
        {
            
            ObjectId choferId = deserealizarJsonToObjectId(idChofer);
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

        [HttpGet]
        [Route("CoordenadasClienteWeb")]
        public JsonResult CoordenadasClienteWeb(string latitud, string longitud)
        {
           
                string idCliente = _session.GetString("UserId");
                if (idCliente != null)
                {
                    return Json(_controladoraViajes.guardarUbicacionCliente(idCliente, latitud, longitud));
                }

            return Json(null);
          
        }

        [HttpGet]
        [Route("CoordenadasCliente")]
        public JsonResult CoordenadasCliente(string idCliente, string latitud, string longitud)
        {
                  ObjectId clienteId = deserealizarJsonToObjectId(idCliente);
            if (!string.IsNullOrEmpty(latitud) && !string.IsNullOrEmpty(longitud)){
                return Json(_controladoraViajes.guardarUbicacionCliente(clienteId.ToString(), latitud, longitud));
            }
            return Json(null);
        }

        [HttpGet]
        [Route("FinalizarViaje")]
        public async Task<JsonResult> FinalizarViaje(Viaje viaje)
        {
            return Json(await _controladoraViajes.finalizarViaje(viaje));
        }
        //[HttpGet]
        //[Route("ConsultaServicioFinalizado")]
        //public async Task<JsonResult> SolicitudServicio(Item item, string latitudCliente, string longitudCliente)
        //{
        //    double unidades = _controladoraVehiculos.calcularUnidades(item.Alto, item.Ancho, item.Profundidad);
        //   Vehiculo vehiculo = await _controladoraVehiculos.mejorVehiculo(latitudCliente, longitudCliente, unidades, item.Peso);
        //    return Json(vehiculo);
        //}
        [HttpGet]
        [Route("VehiculoPrueba")]
        public JsonResult VehiculoPrueba()
        {
            return Json(_controladoraVehiculos.getVehiculo("5b60ad9ab73c94313c6c7552"));
        }
        //devuelve la ubicacion del vehiculo solicitado
        [HttpGet]
        [Route("UbicacionVehiculo")]
        public JsonResult UbicacionVehiculo(string idVehiculo)
        {
            
            return Json((PosicionSatelital)_controladoraVehiculos.UbicacionVehiculos[idVehiculo]);
        }
        //falta impactar el viaje contra la base

        [HttpPost]
        [Route("SolicitudServicio")]
        public async Task<JsonResult> SolicitudServicio([FromBody]Viaje viaje)
        {
            
            Viaje salida = await _controladoraViajes.solicitarViaje(viaje, TipoVehiculo.Otros);
            salida.CostoEstimadoFinal = _controladoraViajes.calcularPrecio(salida.DuracionEstimadaTotal, viaje.Vehiculo.Tarifa,viaje.Compartido);
            return Json(salida);
        }

        [HttpGet]
        [Route("SolicitudMudanza")]
        public async Task<JsonResult> SolicitudMudanza(Viaje viaje)
        {
            Viaje salida = await _controladoraViajes.solicitarViaje(viaje,TipoVehiculo.CamionMudanza);
            return Json(salida);
        }

        [HttpGet]
        [Route("Presupuesto")]
        public async Task<JsonResult>Presupuesto(Presupuesto presupuesto)
        {
            await _controladoraViajes.presupuestoNuevo(presupuesto);
            return Json(presupuesto);
        }

        [HttpGet]
        [Route("ConsultaServicioFinalizado")]
        public async Task<JsonResult>ConsultaServicioFinalizado(string idViaje)
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
        //[HttpGet]
        //[Route("UbicacionClienteChofer")]
        //public JsonResult UbicacionClienteChofer(string idViaje)
        //{

        //}

        //[HttpGet]
        //[Route("ViajesChofer")]
        //public async Task<JsonResult>ViajesChofer(string idChofer)
        //{
        //    return Json(await _controladoraViajes.viajeEnCursoChofer(idChofer));
        //}

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


        //devuelve el costo de la cancelacion.
        //0 si la cancelacion se realiza antes de la confirmacion
        //100 si se realiza luego de la confirmacion
        //-1 si se realiza luego que el chofer llego al origen. no se puede cancelar alli
        [HttpGet]
        [Route("CancelarViaje")]
        public async Task<JsonResult> CancelarViaje(string idViaje)
        {
            ObjectId viajeId = deserealizarJsonToObjectId(idViaje);
            double salida = await _controladoraViajes.cancelarViaje(viajeId.ToString());
            return Json(salida);
        }
        [HttpGet]
        [Route("ConfirmarCancelacion")]
        public async Task ConfirmarCancelacion(string idViaje,string costo)
        {
            double valor = 0;
            double.TryParse(costo, out valor);
            ObjectId viajeId = deserealizarJsonToObjectId(idViaje);
            await _controladoraViajes.confirmarCancelacion(viajeId.ToString(), valor);
        }

        [HttpGet]
        [Route("ConfirmarViaje")]
        public async Task<JsonResult> ConfirmarViaje(string idViaje)
        {
            ObjectId viajeId = deserealizarJsonToObjectId(idViaje);
            Viaje salida = await _controladoraViajes.confirmarViaje(viajeId.ToString());
            return Json(salida);
        }

        [HttpPost]
        [Route("Levantar")]
        public async Task<JsonResult>Levantar([FromBody]Viaje viaje)
        {
            Viaje salida = await _controladoraViajes.levanteViaje(viaje);
            return Json(salida);
        }

        [HttpGet]
        [Route("ViajesCliente")]
        public JsonResult ViajesCliente(string idCliente)
        {
            ObjectId clienteId = deserealizarJsonToObjectId(idCliente);
            return Json(_controladoraViajes.viajesCliente(clienteId.ToString()));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idChofer"></param>
        /// <returns>HISTORICO VIAJES CHOFER</returns>
        [HttpGet]
        [Route("ViajesChofer")]
        public JsonResult ViajesChofer(string idChofer)
        {
            ObjectId choferId = deserealizarJsonToObjectId(idChofer);
            return Json(_controladoraViajes.viajesChofer(choferId.ToString()));
        }
        /// <summary>
        /// VIAJES QUE NO ESTEN FINALIZADOS
        /// </summary>
        /// <param name="idChofer"></param>
        /// <returns>VIAJES ACTIVOS CHOFER</returns>
        [HttpGet]
        [Route("ViajesActivosChofer")]
        public JsonResult ViajesActivosChofer(string idChofer)
        {
            ObjectId choferId = deserealizarJsonToObjectId(idChofer);
            return Json(_controladoraViajes.viajesActivosChofer(choferId.ToString()));
        }
        /// <summary>
        /// DESEREALIZA UN JSON CON EL OBJECTID 
        /// </summary>
        /// <param name="idJson"></param>
        /// <returns>UN OBJETO OBJECTID</returns>
        public ObjectId deserealizarJsonToObjectId(string idJson)
        {
            var aux = BsonDocument.Parse(idJson);
            var timestamp = aux.GetValue("timestamp").ToDouble();
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime time = unixEpoch.AddSeconds(timestamp).ToLocalTime();
            var machine = aux.GetValue("machine").ToInt32();
            var pid = Convert.ToInt16(aux.GetValue("pid").ToInt32());
            var increment = aux.GetValue("increment").ToInt32();
            var creationTime = aux.GetValue("creationTime");
            ObjectId id = new ObjectId(time, machine, pid, increment);
            return id;
        }
        #endregion
    }
}