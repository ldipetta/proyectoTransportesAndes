using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson.IO;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Exceptions;
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
        [Route("RegistroCliente")]
        public async Task<JsonResult> RegistroCliente(Cliente nuevo)
        {
            Cliente cliente = await DBRepositoryMongo<Cliente>.GetUsuario(nuevo.User, "Clientes");
            Chofer chofer = await DBRepositoryMongo<Chofer>.GetUsuario(nuevo.User, "Choferes");
            Usuario usu = await DBRepositoryMongo<Usuario>.GetUsuario(nuevo.User, "Usuarios");

            if (cliente == null && chofer == null && usu == null)
            {
                //Cliente cli = new Cliente(usuario, pass, razonSocial, rut, nombre, apellido, email, documento, telefono, direccion, fNacimiento, numeroTarjCredito, fVencTarjetaCredito);
                await DBRepositoryMongo<Cliente>.Create(nuevo, "Clientes");
                return Json(nuevo);
            }
            else
            {
                return Json(null);
            }
        }
        [Route("RegistroChofer")]
        public async Task<JsonResult> RegistroChofer(Chofer nuevo)
        {
            Usuario usu = await DBRepositoryMongo<Usuario>.GetUsuario(nuevo.User, "Usuarios");
            Usuario cliente = await DBRepositoryMongo<Cliente>.GetUsuario(nuevo.User, "Clientes");
            Usuario chofer = await DBRepositoryMongo<Chofer>.GetUsuario(nuevo.User, "Choferes");

            if (cliente == null && usu == null && chofer == null)
            {
                //Chofer nuevo = new Chofer(usuario, pass, nombre, apellido, email, documento, telefono, direccion, fNacimiento, numero, vtoCarneSalud, categoriaLibreta, fVtoLibreta, foto);
                await DBRepositoryMongo<Chofer>.Create(nuevo, "Choferes");
                return Json(nuevo);
            }
            else
            {
                return Json(null);
            }
        }
        //es el metodo que la app chofer llama en el hilo para actualizar su posicion
        [Route("GuardarCoordenadasVehiculo")]
        public JsonResult GuardarCoordenadasVehiculos(string idVehiculo, string latitud, string longitud)
        {
            return Json(new { Success = true });
        }
        [HttpGet]
        [Route("CoordenadasClienteWeb")]
        public JsonResult CoordenadasClienteWeb(string latitud, string longitud)
        {
            string idCliente = _session.GetString("UserId");
            return Json(_controladoraViajes.guardarUbicacionCliente(idCliente, latitud, longitud));
        }
        [HttpGet]
        [Route("CoordenadasCliente")]
        public JsonResult CoordenadasCliente(string idCliente, string latitud, string longitud)
        {
            return Json(_controladoraViajes.guardarUbicacionCliente(idCliente, latitud, longitud));
        }
        [HttpGet]
        [Route("FinalizarViaje")]
        public async Task<JsonResult> FinalizarViaje(Viaje viaje)
        {
            return Json(await _controladoraViajes.finalizarViaje(viaje));
        }
        [HttpGet]
        [Route("ConsultaServicioFinalizado")]
        public async Task<JsonResult> SolicitudServicio(Item item, string latitudCliente, string longitudCliente)
        {
            double unidades = _controladoraVehiculos.calcularUnidades(item.Alto, item.Ancho, item.Profundidad);
           Vehiculo vehiculo = await _controladoraVehiculos.mejorVehiculoPrueba(latitudCliente, longitudCliente, unidades, item.Peso);
            return Json(vehiculo);
        }
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
        [HttpGet]
        [Route("SolicitudServicio")]
        public async Task<JsonResult> SolicitudServicio(Viaje viaje)
        {
            viaje.DuracionEstimadaHastaCliente = await _controladoraVehiculos.tiempoDemora(viaje.Vehiculo.PosicionSatelital.Latitud, viaje.Vehiculo.PosicionSatelital.Longitud,viaje.DireccionOrigen);
            TimeSpan duracionTotal = TimeSpan.Parse("0");
            TimeSpan tiempoDemora = TimeSpan.Parse("0");
            viaje.CostoEstimadoFinal = _controladoraViajes.calcularPrecio(duracionTotal, viaje.Vehiculo.Tarifa);
            viaje.DuracionEstimadaTotal = duracionTotal;
            return Json(viaje);
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
        [HttpGet]
        [Route("ViajesChofer")]
        public async Task<JsonResult>ViajesChofer(string idChofer)
        {
            return Json(await _controladoraViajes.viajeEnCursoChofer(idChofer));
        }
        #endregion
    }
}