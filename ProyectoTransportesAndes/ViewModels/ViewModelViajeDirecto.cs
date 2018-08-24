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

        public ViewModelViajeDirecto(IOptions<AppSettingsMongo>settings)
        {
            _settings = settings;
            _controladoraUsuarios = ControladoraUsuarios.getInstance(_settings);
            _controladoraVehiculos = ControladoraVehiculos.getInstance(_settings);
            cargarClientes().Wait();
            cargarVehiculos().Wait();
        }
        public ViewModelViajeDirecto() { }

        public async Task cargarClientes()
        {
            var clientes = await ControladoraUsuarios.getInstance(_settings).getClientes();
            List<Cliente> lista = clientes.ToList();
            Cliente c = new Cliente() { Id = new ObjectId(), Nombre = "Seleccione un cliente" };
            lista.Insert(0, c);
            Clientes = new SelectList(lista, "Id", "Leyenda");
        }
        public async Task cargarVehiculos()
        {
            var vehiculos = await ControladoraVehiculos.getInstance(_settings).getVehiculos();
            List<Vehiculo> lista = vehiculos.ToList();
            Vehiculo v = new Vehiculo() { Id = new ObjectId(), Matricula = "Seleccione un vehiculo" };
            lista.Insert(0, v);
            Vehiculos = new SelectList(lista, "Id", "Matricula");
        }
    }
}
