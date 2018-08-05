using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProyectoTransportesAndes.ControllersAPI;
using System.Device.Location;
using System.Net.Http;
using ProyectoTransportesAndes.Persistencia;
using Newtonsoft.Json;
using System.Data;
using ProyectoTransportesAndes.Json;
using Microsoft.Extensions.Options;
using ProyectoTransportesAndes.Configuracion;
using System.Collections;
using MongoDB.Bson;

namespace ProyectoTransportesAndes.Models
{
    public class ControladoraVehiculos
    {
        private IOptions<AppSettingsMongo> _settings;
        private static ControladoraVehiculos _instance;
        Hashtable _ubicacionVehiculos;
        public static ControladoraVehiculos getInstance(IOptions<AppSettingsMongo> settings)
        {
            if (_instance == null)
            {
                _instance = new ControladoraVehiculos(settings);
            }
            return _instance;
        }
        private ControladoraVehiculos(IOptions<AppSettingsMongo> settings)
        {
            _settings = settings;
            _ubicacionVehiculos = new Hashtable();
            DBRepositoryMongo<Vehiculo>.Iniciar(_settings);
            DBRepositoryMongo<Chofer>.Iniciar(_settings);
            datos();
        }
        public async Task<IEnumerable<Vehiculo>> getVehiculos()
        {
            var items = await DBRepositoryMongo<Vehiculo>.GetItemsAsync("Vehiculos");
            return items;
        }
        public async Task<Vehiculo> getVehiculo(string idVehiculo)
        {
            Vehiculo vehiculo = await DBRepositoryMongo<Vehiculo>.GetItemAsync(idVehiculo, "Vehiculos");
            return vehiculo;
        }
        public async Task<Chofer> getChofer(string idChofer)
        {
            Chofer chofer = await DBRepositoryMongo<Chofer>.GetItemAsync(idChofer, "Choferes");
            return chofer;
        }
        public async Task nuevoVehiculo(Vehiculo nuevo)
        {
            try
            {
                await DBRepositoryMongo<Vehiculo>.Create(nuevo, "Vehiculos");
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
        public async Task editarVehiculo(Vehiculo vehiculo, string idVehiculo)
        {
            vehiculo.Id = new ObjectId(idVehiculo);
            await DBRepositoryMongo<Vehiculo>.UpdateAsync(vehiculo.Id, vehiculo, "Vehiculos");
        }
        public async Task eliminarVehiculo(Vehiculo vehiculo, string id)
        {
            vehiculo.Id = new ObjectId(id);
            await DBRepositoryMongo<Vehiculo>.DeleteAsync(vehiculo.Id, "Vehiculos");

        }
        //como primera aproximacion se toman las unidades como m3 de capacidad de carga, el peso de la carga se ira descontando de la capacidad de carga
        public double calcularUnidades(double alto, double ancho, double profundidad)
        {
            return (alto / 100) * (ancho / 100) * (profundidad / 100);
        }
        // devuelve el vehiculo mas cercano al cliente con capacidad de carga para trasladar lo necesario
        public async Task<Vehiculo> mejorVehiculo(string latitudCliente, string longitudCliente, double unidadadesTraslado, double pesoTraslado)
        {
            HttpClient cliente = new HttpClient();
            List<Vehiculo> aux = new List<Vehiculo>();
            List<PosicionSatelital> ubicaciones = obtenerUbicacionesVehiculos();
            int menorTiempo = int.MaxValue;
            Vehiculo vehiculo = null;
            Vehiculo masCercanoConCapacidad = null;
            foreach (PosicionSatelital pos in ubicaciones)
            {
                int tiempo = int.MaxValue;
                string path = "https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=" + latitudCliente + "," + longitudCliente + "&destinations=" + pos.Latitud + "," + pos.Longitud + "&key=AIzaSyB08YiU7GpCk0RCQozZWxiIj3Ud3se0_Ec";
                HttpResponseMessage response = await cliente.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    RootObject obj = JsonConvert.DeserializeObject<RootObject>(await response.Content.ReadAsStringAsync());
                    foreach (Row r in obj.rows)
                    {
                        foreach (Element e in r.elements)
                        {
                            tiempo = int.Parse(e.duration.text.Split(" ")[0]);
                            if (tiempo < menorTiempo)
                            {
                                menorTiempo = tiempo;
                                vehiculo = await DBRepositoryMongo<Vehiculo>.GetItemAsync(pos.Id, "Vehiculos");
                                vehiculo.PosicionSatelital = obtenerUltimaUbicacionVehiculo(vehiculo.Id.ToString());
                                if (vehiculo.Disponible)
                                {
                                    if (masCercanoConCapacidad == null)
                                    {
                                        if (vehiculo.Unidades >= unidadadesTraslado)
                                        {
                                            if (vehiculo.CapacidadCargaKg >= pesoTraslado)
                                            {
                                                masCercanoConCapacidad = vehiculo;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        if (vehiculo.Unidades < masCercanoConCapacidad.Unidades && vehiculo.Unidades >= unidadadesTraslado)
                                        {
                                            if (vehiculo.CapacidadCargaKg < masCercanoConCapacidad.CapacidadCargaKg && vehiculo.CapacidadCargaKg >= pesoTraslado)
                                            {
                                                masCercanoConCapacidad = vehiculo;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }

            }
            return masCercanoConCapacidad;
        }
        public async Task<Vehiculo> mejorVehiculoPrueba(string latitudCliente, string longitudCliente, double unidadadesTraslado, double pesoTraslado)
        {
            List<PosicionSatelital> ubicaciones = obtenerUbicacionesVehiculos();
            int menorTiempo = int.MaxValue;
            Vehiculo vehiculo = null;
            Vehiculo masCercanoConCapacidad = null;
            TimeSpan minutos;
            foreach (PosicionSatelital pos in ubicaciones)
            {
                int tiempo = int.MaxValue;
                minutos = await tiempoDemora(latitudCliente, longitudCliente, pos.Latitud, pos.Longitud);
                tiempo = minutos.Minutes;
                if (tiempo < menorTiempo)
                {
                    menorTiempo = tiempo;
                    vehiculo = await DBRepositoryMongo<Vehiculo>.GetItemAsync(pos.Id, "Vehiculos");
                    vehiculo.PosicionSatelital = obtenerUltimaUbicacionVehiculo(vehiculo.Id.ToString());
                    if (vehiculo.Disponible)
                    {
                        if (masCercanoConCapacidad == null)
                        {
                            if (vehiculo.Unidades >= unidadadesTraslado)
                            {
                                if (vehiculo.CapacidadCargaKg >= pesoTraslado)
                                {
                                    masCercanoConCapacidad = vehiculo;
                                }
                            }
                        }
                        else
                        {
                            if (vehiculo.Unidades < masCercanoConCapacidad.Unidades && vehiculo.Unidades >= unidadadesTraslado)
                            {
                                if (vehiculo.CapacidadCargaKg < masCercanoConCapacidad.CapacidadCargaKg && vehiculo.CapacidadCargaKg >= pesoTraslado)
                                {
                                    masCercanoConCapacidad = vehiculo;
                                }
                            }
                        }
                    }

                }
            }
            return masCercanoConCapacidad;
        }
        public async Task<TimeSpan> tiempoDemora(string latitudCliente, string longitudCliente, string latitudVehiculo, string longitudVehiculo)
        {
            HttpClient cliente = new HttpClient();
            int tiempo = int.MaxValue;
            string path = "https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=" + latitudCliente + "," + longitudCliente + "&destinations=" + latitudVehiculo + "," + longitudVehiculo + "&key=AIzaSyB08YiU7GpCk0RCQozZWxiIj3Ud3se0_Ec";
            HttpResponseMessage response = await cliente.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                RootObject obj = JsonConvert.DeserializeObject<RootObject>(await response.Content.ReadAsStringAsync());
                foreach (Row r in obj.rows)
                {
                    foreach (Element e in r.elements)
                    {
                        tiempo = int.Parse(e.duration.text.Split(" ")[0]);
                    }
                }
            }
            TimeSpan minutes = TimeSpan.FromMinutes(tiempo);
            return minutes;
        }
        public async Task<List<Vehiculo>> vehiculosConPosicion()
        {
            List<PosicionSatelital> posiciones = obtenerUbicacionesVehiculos();
            List<Vehiculo> salida = new List<Vehiculo>();
            foreach (PosicionSatelital pos in posiciones)
            {
                Vehiculo v = await DBRepositoryMongo<Vehiculo>.GetItemAsync(pos.Id, "Vehiculos");
                if (v != null)
                {
                    v.PosicionSatelital = pos;
                    salida.Add(v);
                }

            }
            return salida;
        }
        public async Task<IEnumerable<Chofer>> choferesDisponibles()
        {
            var lista = await DBRepositoryMongo<Chofer>.GetItemsAsync("Choferes");
            IEnumerable<Chofer> choferes = lista.Where(c => c.Disponible == true);
            return choferes;
        }
        public PosicionSatelital guardarUbicacionVehiculo(string idVehiculo, string latitud, string longitud)
        {
            PosicionSatelital posicion = new PosicionSatelital(idVehiculo, latitud, longitud);
            if (_ubicacionVehiculos.Contains(idVehiculo))
            {
                _ubicacionVehiculos[idVehiculo] = posicion;
            }
            else
            {
                _ubicacionVehiculos.Add(idVehiculo, posicion);
            }
            return posicion;
        }
        public List<PosicionSatelital> obtenerUbicacionesVehiculos()
        {
            List<PosicionSatelital> ubicaciones = new List<PosicionSatelital>();
            if (_ubicacionVehiculos.Count > 0)
            {
                foreach (DictionaryEntry d in _ubicacionVehiculos)
                {
                    ubicaciones.Add((PosicionSatelital)d.Value);
                }
            }
            return ubicaciones;
        }
        public PosicionSatelital obtenerUltimaUbicacionVehiculo(string idVehiculo)
        {
            return (PosicionSatelital)_ubicacionVehiculos[idVehiculo];
        }
        public void datos()
        {
            PosicionSatelital hard = new PosicionSatelital("5b60ad9ab73c94313c6c7552", "-34.890875", "-56.137279");
            _ubicacionVehiculos["5b60ad9ab73c94313c6c7552"] = hard;
        }
    }
}
