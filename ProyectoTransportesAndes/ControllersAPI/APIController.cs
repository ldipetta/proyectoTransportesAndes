using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Exceptions;
using ProyectoTransportesAndes.Models;
using ProyectoTransportesAndes.Persistencia;

namespace ProyectoTransportesAndes.ControllersAPI
{
    [Produces("application/json")]
    [Route("api/ClienteAPI")]
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
        public async Task<JsonResult> RegistroCliente(string usuario, string pass, string razonSocial, string rut, string nombre, string apellido, string email, string documento, string telefono, string direccion, string fNacimiento, string numeroTarjCredito, string fVencTarjetaCredito)
        {
            Cliente cliente = await DBRepositoryMongo<Cliente>.GetUsuario(usuario, "Clientes");
            Chofer chofer = await DBRepositoryMongo<Chofer>.GetUsuario(usuario, "Choferes");
            Usuario usu = await DBRepositoryMongo<Usuario>.GetUsuario(usuario, "Usuarios");

            if (cliente == null && chofer == null && usu == null)
            {
                Cliente nuevo = new Cliente(usuario, pass, razonSocial, rut, nombre, apellido, email, documento, telefono, direccion, fNacimiento, numeroTarjCredito, fVencTarjetaCredito);
                await DBRepositoryMongo<Cliente>.Create(nuevo, "Clientes");
                return Json(nuevo);
            }
            else
            {
                return Json(null);
            }
        }
        [Route("RegistroChofer")]
        public async Task<JsonResult> RegistroChofer(string usuario, string pass, string nombre, string apellido, string email, string documento, string telefono, string direccion, string fNacimiento, string numero, string vtoCarneSalud, string categoriaLibreta, string fVtoLibreta, string foto)
        {
            Usuario usu = await DBRepositoryMongo<Usuario>.GetUsuario(usuario, "Usuarios");
            Usuario cliente = await DBRepositoryMongo<Cliente>.GetUsuario(usuario, "Clientes");
            Usuario chofer = await DBRepositoryMongo<Chofer>.GetUsuario(usuario, "Choferes");

            if (cliente == null && usu == null && chofer == null)
            {
                Chofer nuevo = new Chofer(usuario, pass, nombre, apellido, email, documento, telefono, direccion, fNacimiento, numero, vtoCarneSalud, categoriaLibreta, fVtoLibreta, foto);
                await DBRepositoryMongo<Chofer>.Create(nuevo, "Choferes");
                return Json(nuevo);
            }
            else
            {
                return Json(null);
            }
        }
        //La idea es que todos los dispositivos devuelvan su ubicacion. Observer?
        [Route("UbicacionVehiculo")]
        public JsonResult CoordenadasVehiculos(string idVehiculo, string latitud, string longitud)
        {
            return Json(_controladoraVehiculos.guardarUbicacionVehiculo(idVehiculo, latitud, longitud));
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
        [Route("FinalizarViaje")]
        public async Task<JsonResult> FinalizarViaje(Viaje viaje)
        {
            return Json(await _controladoraViajes.finalizarViaje(viaje));
            //habria que implementar una respuesta a todos los usuarios con signalr
        }

        #endregion
    }
}