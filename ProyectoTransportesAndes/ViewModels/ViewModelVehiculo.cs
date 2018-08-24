using Microsoft.AspNetCore.Mvc.Rendering;
using ProyectoTransportesAndes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProyectoTransportesAndes.Persistencia;
using ProyectoTransportesAndes.Configuracion;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace ProyectoTransportesAndes.ViewModels
{
    public class ViewModelVehiculo
    {
        public SelectList ListaChoferes { get; set; }
        public string ChoferSeleccionado { get; set; }
        public Vehiculo Vehiculo { get; set; }
        private IOptions<AppSettingsMongo> _settings;


        public ViewModelVehiculo(IOptions<AppSettingsMongo> settings)
        {
            _settings = settings;
            cargarDatos().Wait();
        }
        public ViewModelVehiculo()
        {
          
        }
        public async Task cargarDatos()
        {
            var choferes = await ControladoraVehiculos.getInstance(_settings).choferesDisponibles();
            ListaChoferes = new SelectList(choferes, "Id", "Leyenda");
        }


    }
}
