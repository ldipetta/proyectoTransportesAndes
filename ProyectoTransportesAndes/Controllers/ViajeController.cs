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

namespace ProyectoTransportesAndes.Controllers
{
    [Produces("application/json")]
    [Route("api/Viaje")]
    public class ViajeController : Controller
    {
        private IOptions<AppSettingsMongo> _settings;
        private readonly IConfiguration _configuration;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;
        private ControladoraViajes _controladoraViajes;
        private ControladoraVehiculos _controladoraVehiculos;

        public ViajeController(IOptions<AppSettingsMongo> settings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _configuration = configuration;
            _httpContext = httpContextAccessor;
            _settings = settings;
            _controladoraViajes = ControladoraViajes.getInstancia(_settings);
            _controladoraVehiculos = ControladoraVehiculos.getInstance(_settings);
        }

        [HttpGet]
        [Route("SolicitarViaje")]
        //las coordenadas origen estan hardodeadas, deberian levantarse del dispositivo del cliente
        public async Task<IActionResult> SolicitarViaje()
        {
            ViewModelViaje model = new ViewModelViaje();
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

        [HttpPost]
        [Route("SolicitarViaje")]
        public async Task<IActionResult> SolicitarViaje(string idViaje)
        {
            //string latitudCliente = "";
            //latitudCliente = _session.GetString("latitudCliente");
            //string longitudCliente = "";
            //longitudCliente = _session.GetString("longitudCliente");
            string idCliente = _session.GetString("UserId");

          
                if (idViaje!=null)
                {
                    Viaje viaje = await _controladoraViajes.solicitarViaje(idViaje, idCliente);

                    if (viaje == null)
                    {
                        ViewBag.Mensaje("No hay vehiculos disponibles. Estamos buscando un vehiculo para usted.");
                        return RedirectToAction("SolicitarViaje");
                    }
                    else
                    {
                        return RedirectToAction("ResumenViaje", new { viaje.Id });
                    }
                }
                else
                {
                    return RedirectToAction("SolicitarViaje");
                }

        }

        [HttpPost]
        [Route("AgregarItem")]
        public async Task<IActionResult> AgregarItem(ViewModelViaje model)
        {
            string idCliente = _session.GetString("UserId");
            model.Item.Tipo = model.TipoItem;
            await _controladoraViajes.agregarItem(idCliente, model.Item);
            return RedirectToAction("SolicitarViaje");
        }

        [HttpGet]
        [Route("EditarItem")]
        public async Task<IActionResult> EditarItem(string itemId)
        {
            string idCliente = _session.GetString("UserId");
            ViewModelViaje model = new ViewModelViaje();
            var item = await _controladoraViajes.itemParaEditar(idCliente, itemId);
            model.Item = item;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditarItem(ViewModelViaje model)
        {
            string idCliente = _session.GetString("UserId");
            await _controladoraViajes.editarItem(idCliente, model.Item);
            return RedirectToAction("SolicitarViaje");
        }


        [HttpGet]
        [Route("ResumenViaje")]
        public async Task<IActionResult> ResumenViaje(string id)
        {
            //armar vista en base al id del viaje. con eso se saca la tarifa etc etc
            Viaje enCurso = await _controladoraViajes.getViaje(id);
            ViewModelViaje resumen = new ViewModelViaje();
            if (enCurso != null)
            {
                
                resumen.Viaje = enCurso;
                resumen.PrecioEstimado = _controladoraViajes.calcularPrecio(enCurso.DuracionEstimada,enCurso.Vehiculo.Tarifa);
                resumen.LatitudOrigen = _controladoraViajes.obtenerUbicacionCliente(enCurso.Cliente.Id.ToString()).Latitud;
                resumen.LongitudOrigen = _controladoraViajes.obtenerUbicacionCliente(enCurso.Cliente.Id.ToString()).Longitud;
                resumen.HoraEstimadaLlegada = string.Format("{0:hh\\:mm}", enCurso.HoraInicio + enCurso.DuracionEstimada);
                resumen.HoraInicio = string.Format("{0:hh\\:mm}", enCurso.HoraInicio);
               
            }
            else
            {
                return RedirectToAction("SolicitarViaje");
            }

            return View(resumen);
        }

        [HttpPost]
        [Route("FinalizarViaje")]
        public async Task FinalizarViaje(ViewModelViaje model)
        {
            await _controladoraViajes.finalizarViaje(model.Viaje);
            //habria que actualizar las vistas de resumen viaje con un observer por ej.
        }

        [HttpGet]
        public IActionResult MisViajes()
        {
            var token = _session.GetString("Token");
            if (token != null)
            {
                var rol = Usuario.validarToken(token);
                if (rol.Equals("Cliente"))
                {
                    var idCliente = _session.GetString("UserId");
                    return View(_controladoraViajes.viajesCliente(idCliente));
                }
                else
                {
                    return BadRequest("No posee los permisos");
                }
            }
            else
            {
                return BadRequest("Debe iniciar sesión");
            }
        }

    }
}