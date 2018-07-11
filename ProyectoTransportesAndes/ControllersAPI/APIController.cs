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

namespace ProyectoTransportesAndes.ControllersAPI
{
    [Produces("application/json")]
    [Route("api/ClienteAPI")]
    public class APIController : Controller
    {
        
        private IOptions<AppSettingsMongo> _settings;
        private readonly IConfiguration _configuration;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;

        public APIController(IOptions<AppSettingsMongo> settings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _configuration = configuration;
            _httpContext = httpContextAccessor;
            _settings = settings;

            DBRepositoryMongo<Usuario>.Iniciar(_settings);
            DBRepositoryMongo<Cliente>.Iniciar(_settings);
            DBRepositoryMongo<Chofer>.Iniciar(_settings);
        }

        //Servicio para loguearse desde las Apps móviles, devuelve un json con datos del usuario para
        //ser utilizados por las Apps
        //Que pasa con el token? se pude levantar la variable session en xamarin?
        [HttpGet]
        [Route("LoginAPP")]
        public async Task<JsonResult> LoginAPP(string usuario, string pass)
        {
            Usuario user = null;
            if (ModelState.IsValid)
            {
                user = await DBRepositoryMongo<Usuario>.Login(usuario, pass);
                if (user != null)
                {
                    if (user.Password == pass)
                    {
                        _session.SetString("Token", Usuario.BuildToken(user/*, _configuration*/));
                        _session.SetString("User", usuario);
                        return Json(user);
                    }
                    else
                    {
                        return Json(user);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                    return Json(user);
                }
            }
            else
            {
                return Json(user);
            }

        }
        [Route("RegistroCliente")]
        public async Task<JsonResult> RegistroCliente(string usuario, string pass, string razonSocial,string rut, string nombre, string apellido, string email, string documento, string telefono, string direccion, string fNacimiento, string numeroTarjCredito, string fVencTarjetaCredito)
        {
            Cliente cliente = await DBRepositoryMongo<Cliente>.GetUsuario(usuario, "Clientes");
            Chofer chofer = await DBRepositoryMongo<Chofer>.GetUsuario(usuario, "Choferes");
            Usuario usu = await DBRepositoryMongo<Usuario>.GetUsuario(usuario, "Usuarios");

            if (cliente == null && chofer == null && usu == null)
            { 
                Cliente nuevo = new Cliente(usuario,pass,razonSocial,rut,nombre,apellido,email,documento,telefono,direccion,fNacimiento,numeroTarjCredito,fVencTarjetaCredito);
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

            if (cliente == null && usu==null && chofer==null)
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
    }
}