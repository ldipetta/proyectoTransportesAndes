using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.ViewModels
{
    public class ViewModelViajeDirecto
    {
        private IOptions<AppSettingsMongo> _settings;
        private ControladoraUsuarios _controladoraUsuarios;
        private ControladoraVehiculos _controladoraVehiculos;
        public SelectList Vehiculos { get; set; }
        public string VehiculoId { get; set; }
        public SelectList Clientes { get; set; }
        public string ClienteId { get; set; }
        public string Direccion { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string Comentarios { get; set; }
        public double CostoFinal { get; set; }
        public bool UtilizarDireccionCliente { get; set; }
        public string FechaParaMostrar { get; set; }

        public ViewModelViajeDirecto(IOptions<AppSettingsMongo>settings)
        {
            _settings = settings;
            _controladoraUsuarios = ControladoraUsuarios.getInstance(_settings);
            _controladoraVehiculos = ControladoraVehiculos.getInstance(_settings);
            cargarClientes().Wait();
            cargarVehiculos();
        }
        public ViewModelViajeDirecto() { }

        public async Task cargarClientes()
        {
            var clientes = await ControladoraUsuarios.getInstance(_settings).getClientes();
            List<Cliente> lista = clientes.ToList();
            Cliente c = new Cliente() { Id = new ObjectId(), Leyenda = "Seleccione un cliente" };
            lista.Insert(0, c);
            Clientes = new SelectList(lista, "Id", "Leyenda");
        }
        public void cargarVehiculos()
        {
            List<Vehiculo> vehiculos = ControladoraVehiculos.getInstance(_settings).getVehiculosDisponibles();
            Vehiculo v = new Vehiculo() { Id = new ObjectId(), Matricula = "Seleccione un vehiculo" };
            vehiculos.Insert(0, v);
            Vehiculos = new SelectList(vehiculos, "Id", "Matricula");
        }
    }
}
