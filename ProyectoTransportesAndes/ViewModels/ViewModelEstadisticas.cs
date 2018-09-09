using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.ViewModels
{
    public class ViewModelEstadisticas
    {
        public SelectList Vehiculos { get; set; }
        public string IdVehiculo { get; set; }
        public SelectList Choferes { get; set; }
        public string IdChofer { get; set; }
        public SelectList Meses { get; set; }
        public SelectList Años { get; set; }
        public string MesSeleccionado { get; set; }
        public string AñoSeleccionado { get; set; }
        public EstadisticaVehiculo Estadistica{get;set;}
        private IOptions<AppSettingsMongo> _settings;

        public ViewModelEstadisticas(IOptions<AppSettingsMongo> settings)
        {
            _settings = settings;
            cargarVehiculos().Wait();
            CargarMes();
            CargarAño();
        }
        public ViewModelEstadisticas()
        {
           
            cargarVehiculos().Wait();
            CargarMes();
            CargarAño();
        }

        public async Task cargarVehiculos()
        {
            var vehiculos = await ControladoraVehiculos.getInstance(_settings).getVehiculos();
            List<Vehiculo> lista = vehiculos.ToList();
            Vehiculo v = new Vehiculo() { Id = new ObjectId(), Matricula = "Seleccione un vehiculo" };
            lista.Insert(0, v);
            Vehiculos = new SelectList(lista, "Id", "Matricula");
        }
        private void CargarMes()
        {
           
            var selectList = new List<SelectListItem>();
            List<string> nombreMes = DateTimeFormatInfo.CurrentInfo.MonthNames.Take(12).ToList();
            selectList.Add(new SelectListItem { Value = "", Text = "Seleccione un mes" });

            foreach (string i in nombreMes)
            {
                selectList.Add(new SelectListItem
                {
                    Value = (nombreMes.IndexOf(i) + 1).ToString(),
                    Text = i.ToString()
                });
            }
            Meses = new SelectList(selectList, "Value", "Text");
        }
        private void CargarAño()
        {
             var selectList = new List<SelectListItem>();
            selectList.Add(new SelectListItem { Value = "", Text = "Seleccione un año" });

            int i;
            for (i=1990; i <= DateTime.Now.Year; i++)
            {
                selectList.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = i.ToString()
                });
            }
            Años = new SelectList(selectList, "Value", "Text");
        }

    }
}
