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

namespace ProyectoTransportesAndes.Controllers
{
    [Produces("application/json")]
    [Route("api/Viaje")]
    public class ViajeController : Controller
    {
        private string coleccion = "Viajes";
        private IOptions<AppSettingsMongo> _settings;
        private readonly IConfiguration _configuration;
        private readonly ISession _session;
        private readonly IHttpContextAccessor _httpContext;

        public ViajeController(IOptions<AppSettingsMongo> settings, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _configuration = configuration;
            _httpContext = httpContextAccessor;
            _settings = settings;

            DBRepositoryMongo<Viaje>.Iniciar(_settings);
        }


    }
}