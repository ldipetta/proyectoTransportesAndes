using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Exceptions;
using ProyectoTransportesAndes.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.ViewModels
{
    public class ViewModelViajeFiltro
    {
        [Display(Name = "Estado")]
        public SelectList ListaEstadoViaje { get; set; }
        [Display(Name = "Cliente")]
        public SelectList ListaClientes { get; set; }
        public string IdCliente { get; set; }
        [Display(Name = "Vehiculos")]
        public SelectList ListaVehiculos { get; set; }
        public string IdVehiculo { get; set; }
        public EstadoViaje EstadoViaje { get; set; }
        [DataType(DataType.Date)]
        public DateTime Desde { get; set; } = DateTime.Today;
        [DataType(DataType.Date)]
        public DateTime Hasta { get; set; } = DateTime.Today;
        public IEnumerable<Viaje> Viajes { get; set; }
        private IOptions<AppSettingsMongo> _settings;

        public ViewModelViajeFiltro(IOptions<AppSettingsMongo> settings)
        {
            _settings = settings;

            cargarClientes().Wait();
            cargarVehiculos().Wait();
            cargarEstados();
            Viajes = new List<Viaje>();
        }
        public ViewModelViajeFiltro()
        {
            cargarClientes().Wait();
            cargarVehiculos().Wait();
            cargarEstados();
        }

        public async Task cargarClientes()
        {
            var clientes = await ControladoraUsuarios.getInstance(_settings).getClientes();
            List<Cliente> lista = clientes.ToList();
            Cliente c = new Cliente() { Id = new ObjectId(), Nombre = "Seleccione un cliente" };
            lista.Insert(0, c);
            ListaClientes = new SelectList(lista, "Id", "Leyenda");
        }
        public async Task cargarVehiculos()
        {
            var vehiculos = await ControladoraVehiculos.getInstance(_settings).getVehiculos();
            List<Vehiculo> lista = vehiculos.ToList();
            Vehiculo v = new Vehiculo() { Id = new ObjectId(), Matricula = "Seleccione un vehiculo" };
            lista.Insert(0, v);
            ListaVehiculos = new SelectList(lista, "Id", "Matricula");
        }
        public void cargarEstados()
        {
            try
            {
                var selectList = new List<SelectListItem>();

                var enumValues = Enum.GetValues(typeof(EstadoViaje)) as EstadoViaje[];

                foreach (var enumValue in enumValues)
                {
                    selectList.Add(new SelectListItem
                    {
                        Value = enumValue.ToString(),
                        Text = GetItemName(enumValue)
                    });
                }
                ListaEstadoViaje = new SelectList(selectList, "Value", "Value");
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
        private string GetItemName(EstadoViaje value)
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
