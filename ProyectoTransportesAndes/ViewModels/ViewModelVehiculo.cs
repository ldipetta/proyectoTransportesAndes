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
        public SelectList ListaTipoVehiculo { get; set; }
        public TipoVehiculo TipoVehiculo { get; set; }
        public string Id { get; set; }

        public ViewModelVehiculo(IOptions<AppSettingsMongo> settings)
        {
            _settings = settings;
            cargarDatos().Wait();
            cargarLista();
        }
        public ViewModelVehiculo()
        {
          
        }
        public async Task cargarDatos()
        {
            var choferes = await ControladoraVehiculos.getInstance(_settings).choferesDisponibles();
            ListaChoferes = new SelectList(choferes, "Id", "Leyenda");
        }
        private void cargarLista()
        {
            var selectList = new List<SelectListItem>();

            var enumValues = Enum.GetValues(typeof(TipoVehiculo)) as TipoVehiculo[];

            foreach (var enumValue in enumValues)
            {
                selectList.Add(new SelectListItem
                {
                    Value = enumValue.ToString(),
                    Text = GetItemName(enumValue)
                });
            }
            ListaTipoVehiculo = new SelectList(selectList, "Value", "Value");
        }
        private string GetItemName(TipoVehiculo value)
        {
            var itemInfo = value.GetType().GetMember(value.ToString());
            if (itemInfo.Length != 1)
                return null;

            var displayAttribute = itemInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false)
                                   as DisplayAttribute[];
            if (displayAttribute == null || displayAttribute.Length != 1)
                return null;

            return displayAttribute[0].Name;
        }


    }
}
