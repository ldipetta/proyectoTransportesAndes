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
using ProyectoTransportesAndes.Exceptions;
using static ProyectoTransportesAndes.Json.DireccionToCoordenadas;
using static ProyectoTransportesAndes.Json.TiempoDemoraWaypoints;

namespace ProyectoTransportesAndes.Models
{
    public class ControladoraVehiculos
    {
        #region Atributos
        private IOptions<AppSettingsMongo> _settings;
        private static ControladoraVehiculos _instance;
        private ControladoraViajes _controladoraViajes;
        public Hashtable UbicacionVehiculos { get; set; }
        #endregion

        #region Propiedades
        public static ControladoraVehiculos getInstance(IOptions<AppSettingsMongo> settings)
        {
            if (_instance == null)
            {
                _instance = new ControladoraVehiculos(settings);
            }
            return _instance;
        }
        public List<Vehiculo> Vehiculos { get; set; }
        #endregion

        #region Constructores
        private ControladoraVehiculos(IOptions<AppSettingsMongo> settings)
        {
            _settings = settings;
            _controladoraViajes = ControladoraViajes.getInstancia(_settings);
            UbicacionVehiculos = new Hashtable();
            DBRepositoryMongo<Vehiculo>.Iniciar(_settings);
            DBRepositoryMongo<Chofer>.Iniciar(_settings);
            cargarVehicuos();
            datos();
        }
        #endregion

        #region Metodos
        public async Task<IEnumerable<Vehiculo>> getVehiculos()
        {
            try
            {
                var items = await DBRepositoryMongo<Vehiculo>.GetItemsAsync("Vehiculos");
                return items;
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
        public Vehiculo getVehiculo(string idVehiculo)
        {
            try
            {
                if (idVehiculo != null)
                {
                    Vehiculo vehiculo = null;
                    //Vehiculo vehiculo = await DBRepositoryMongo<Vehiculo>.GetItemAsync(idVehiculo, "Vehiculos");
                    foreach (Vehiculo v in Vehiculos)
                    {
                        if (v.Id.ToString().Equals(idVehiculo))
                        {
                            vehiculo = v;
                        }
                    }
                    return vehiculo;
                }
                else
                {
                    throw new MensajeException("El Id del vehiculo no existe");
                }
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
        public async Task<Chofer> getChofer(string idChofer)
        {
            try
            {
                if (idChofer != null)
                {
                    Chofer chofer = await DBRepositoryMongo<Chofer>.GetItemAsync(idChofer, "Choferes");
                    return chofer;
                }
                else
                {
                    throw new MensajeException("El Id del chofer no existe");
                }
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
        public async Task nuevoVehiculo(Vehiculo vehiculo, string idChofer)
        {
            try
            {
                if (vehiculo != null && idChofer != null)
                {
                    Vehiculo nuevo = vehiculo;
                    Chofer chofer = await getChofer(idChofer);
                    chofer.Disponible = false;
                    nuevo.Chofer = chofer;
                    nuevo.Disponible = true;
                    nuevo.Unidades = calcularUnidades(nuevo.Largo, nuevo.Ancho, nuevo.Alto);
                    await DBRepositoryMongo<Vehiculo>.Create(nuevo, "Vehiculos");
                    Vehiculos.Add(nuevo);
                }
                else
                {
                    throw new MensajeException("Ha ocurrido un error inesperado. Vuelva a intentarlo mas tarde");
                }

            }
            catch (MensajeException msg)
            {
                throw msg;
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
        public async Task editarVehiculo(Vehiculo vehiculo, string idVehiculo, string choferSeleccionado,TipoVehiculo tipoVehiculo)
        {
            try
            {
                if (vehiculo != null && idVehiculo != null)
                {
                    vehiculo.Id = new ObjectId(idVehiculo);
                    vehiculo.Unidades = calcularUnidades(vehiculo.Alto, vehiculo.Ancho, vehiculo.Largo);
                    vehiculo.Tipo = tipoVehiculo;
                    Chofer chofer = await DBRepositoryMongo<Chofer>.GetItemAsync(choferSeleccionado, "Choferes");
                    if (chofer != null)
                    {
                        vehiculo.Chofer = chofer;
                    }
                    await DBRepositoryMongo<Vehiculo>.UpdateAsync(vehiculo.Id, vehiculo, "Vehiculos");
                    Vehiculo eliminar = null;
                    foreach (Vehiculo v in Vehiculos)
                    {
                        if (v.Id.ToString().Equals(idVehiculo))
                        {
                            eliminar = v;
                           
                        }
                    }
                    Vehiculos.Remove(eliminar);
                    Vehiculos.Add(vehiculo);
                }
                else
                {
                    throw new MensajeException("Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                }

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
        public async Task eliminarVehiculo(string id, Vehiculo vehiculo)
        {
            try
            {
                if (vehiculo != null && id != null)
                {
                    vehiculo.Id = new ObjectId(id);
                    await DBRepositoryMongo<Vehiculo>.DeleteAsync(vehiculo.Id, "Vehiculos");
                    Vehiculos.Remove(vehiculo);
                }
                else
                {
                    throw new MensajeException("Ha ocurrido un error inesperado vuelva a intentarlo mas tarde");
                }
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
        //como primera aproximacion se toman las unidades como m3 de capacidad de carga, el peso de la carga se ira descontando de la capacidad de carga. En una segunda instancia se podra tener en cuenta las dimensiones en particular
        // para determinados objetos que deban ir en alguna posicion en particular
        public double calcularUnidades(double alto, double ancho, double profundidad)
        {
            try
            {
                return (alto / 100) * (ancho / 100) * (profundidad / 100);
            }
            catch (ArithmeticException a)
            {
                throw a;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<Vehiculo> mejorVehiculoMudanza(string latitudCliente, string longitudCliente)
        {
            try
            {
                List<PosicionSatelital> ubicaciones = obtenerUbicacionesVehiculosMudanza();
                int menorTiempo = int.MaxValue;
                Vehiculo vehiculo = null;
                Vehiculo masCercano = null;
                TimeSpan minutos;
                foreach (PosicionSatelital pos in ubicaciones)
                {
                    int tiempo = int.MaxValue;
                    minutos = await tiempoDemora(latitudCliente, longitudCliente, pos.Latitud, pos.Longitud);
                    tiempo = minutos.Minutes;
                    if (tiempo < menorTiempo)
                    {
                        menorTiempo = tiempo;
                        vehiculo = getVehiculo(pos.Id);
                        vehiculo.PosicionSatelital = obtenerUltimaUbicacionVehiculo(vehiculo.Id.ToString());
                        if (vehiculo.Disponible)
                        {
                            if (masCercano == null)
                            {
                                masCercano = vehiculo;
                            }
                        }
                    }
                }
                return masCercano;
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
        // devuelve el vehiculo mas cercano al cliente con capacidad de carga para trasladar lo necesario
        public async Task<Vehiculo> mejorVehiculoFlete(string latitudCliente, string longitudCliente, double unidadadesTraslado, double pesoTraslado)
        {
            try
            {
                List<PosicionSatelital> ubicaciones = obtenerUbicacionesVehiculosFletes();
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
                        vehiculo = getVehiculo(pos.Id);
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
                        //else
                        //{
                        //    //return null;
                        //    //throw new MensajeException("El vehiculo no se encuentra disponible");
                        //}
                    }
                }
                return masCercanoConCapacidad;
            }
            catch (TimeoutException)
            {
                throw new MensajeException("Se agoto el tiempo de espera. Compruebe la conexion");
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
        // esta sobrecarga de tiempo de demora se utiliza para calcular el tiempo de demora del vehiculo hasta la posicion del cliente
        public async Task<TimeSpan> tiempoDemora(string latitudCliente, string longitudCliente, string latitudVehiculo, string longitudVehiculo)
        {
            try
            {
                HttpClient cliente = new HttpClient();
                int tiempo = int.MaxValue;
                string path = "https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=" + latitudCliente + "," + longitudCliente + "&destinations=" + latitudVehiculo + "," + longitudVehiculo + "&key=AIzaSyB08YiU7GpCk0RCQozZWxiIj3Ud3se0_Ec";
                HttpResponseMessage response = await cliente.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    VehiculoCercano obj = JsonConvert.DeserializeObject<VehiculoCercano>(await response.Content.ReadAsStringAsync());
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
            catch (TimeoutException)
            {
                throw new MensajeException("Se agoto el tiempo de espera, vuelva a intentarlo en unos minutos");
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
        // esta sobrecarga de tiempo de demora se utiliza para calcular la distancia del cliente a una direccion establecida por este
        public async Task<TimeSpan> tiempoDemora(string latitudX, string longitudX, string direccion)
        {
            HttpClient cliente = new HttpClient();
            int tiempo = int.MaxValue;
            string path = "https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=" + latitudX + "," + longitudX + "&destinations=" + direccion + "&key=AIzaSyB08YiU7GpCk0RCQozZWxiIj3Ud3se0_Ec";
            HttpResponseMessage response = await cliente.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                VehiculoCercano obj = JsonConvert.DeserializeObject<VehiculoCercano>(await response.Content.ReadAsStringAsync());
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
        public async Task<TimeSpan> tiempoDemoraTotal(Viaje viaje)
        {
            try
            {
                HttpClient cliente = new HttpClient();
                int tiempo = int.MaxValue;
                string path = null;
                if (!string.IsNullOrEmpty(viaje.DireccionDestino))
                {
                    path = "https://maps.googleapis.com/maps/api/directions/json?origin=-34.891064, -56.137232&destination=-34.902633, -56.158664";
                }
                if (viaje.Items.Count > 10)
                {
                    throw new MensajeException("Debe ingresar como máximo 10 items");
                }
                else
                {
                    if (viaje.Items.Count != 0)
                    {
                        path += "&waypoints = optimize:true";
                        foreach (Item i in viaje.Items)
                        {
                            path += "|via:" + i.Origen.Latitud + ", " + i.Origen.Longitud + "|via:" + i.Destino.Latitud + ", " + i.Destino.Longitud;
                        }
                    }
                 
                    path += "&key=AIzaSyB08YiU7GpCk0RCQozZWxiIj3Ud3se0_Ec";
                }
                
                HttpResponseMessage response = await cliente.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    DemoraWaypoints obj = JsonConvert.DeserializeObject<DemoraWaypoints>(await response.Content.ReadAsStringAsync());
                    Route route = obj.routes[0];
                    Leg leg = route.legs[0];
                    tiempo = int.Parse(leg.duration.text.Split(" ")[0]);
                }
                TimeSpan minutes = TimeSpan.FromMinutes(tiempo);
                return minutes;
            }
            catch (TimeoutException)
            {
                throw new MensajeException("Se agoto el tiempo de espera, vuelva a intentarlo en unos minutos");
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
        public async Task<PosicionSatelital> convertirDireccionEnCoordenadas(string direccion)
        {
            PosicionSatelital posicion = new PosicionSatelital();
            HttpClient cliente = new HttpClient();
            string path = "https://maps.googleapis.com/maps/api/geocode/json?address=" + direccion + "&key=AIzaSyB08YiU7GpCk0RCQozZWxiIj3Ud3se0_Ec";

            HttpResponseMessage response = await cliente.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                RootObject root = JsonConvert.DeserializeObject<RootObject>(await response.Content.ReadAsStringAsync());
                foreach (Result r in root.results)
                {
                    posicion.Latitud = Convert.ToString(r.geometry.location.lat, System.Globalization.CultureInfo.InvariantCulture);
                    posicion.Longitud = Convert.ToString(r.geometry.location.lng, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            return posicion;
        }
        public async Task<string> convertirCoordenadasEnDireccion(string latitud, string longitud)
        {
            HttpClient cliente = new HttpClient();
            string direccion = "";
            string path = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + latitud + ", " + longitud + "&key=AIzaSyB08YiU7GpCk0RCQozZWxiIj3Ud3se0_Ec";

            HttpResponseMessage response = await cliente.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                RootObject root = JsonConvert.DeserializeObject<RootObject>(await response.Content.ReadAsStringAsync());
                direccion = root.results[0].formatted_address;
                //foreach (Result r in root.results)
                //{
                //    direccion = r.formatted_address;
                //}
            }
            return direccion;
        }
        //devuelve la lista de vehiculos que hay en el sistema con su posicion asociada
        public async Task<List<Vehiculo>> vehiculosConPosicion()
        {
            try
            {
                List<PosicionSatelital> posiciones = obtenerUbicacionesVehiculosFletes();
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
            catch (MensajeException msg)
            {
                throw msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        // devuelve la lista de choferes que estan sin un vehiculo asociado
        public async Task<IEnumerable<Chofer>> choferesDisponibles()
        {
            try
            {
                var lista = await DBRepositoryMongo<Chofer>.GetItemsAsync("Choferes");
                IEnumerable<Chofer> choferes = lista.Where(c => c.Disponible == true);
                return choferes;
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
        // desde los api se manda guardar la ubicacion actual del vehiculo en un hash
        public PosicionSatelital guardarUbicacionVehiculo(string idVehiculo, string latitud, string longitud)
        {
            try
            {
                PosicionSatelital posicion = new PosicionSatelital(idVehiculo, latitud, longitud);
                if (UbicacionVehiculos.Contains(idVehiculo))
                {
                    UbicacionVehiculos[idVehiculo] = posicion;
                }
                else
                {
                    UbicacionVehiculos.Add(idVehiculo, posicion);
                }
                return posicion;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        //devuelve una lista de las posiciones de los vehiculos obtenidas del hash en memoria
        public List<PosicionSatelital> obtenerUbicacionesVehiculosFletes()
        {
            try
            {
                List<PosicionSatelital> ubicaciones = new List<PosicionSatelital>();
                if (Vehiculos.Count > 0)
                {
                    foreach (Vehiculo v in Vehiculos)
                    {
                        if (UbicacionVehiculos.Count > 0)
                        {
                            if (v.Tipo != TipoVehiculo.CamionMudanza)
                            {
                                ubicaciones.Add((PosicionSatelital)UbicacionVehiculos[v.Id.ToString()]);
                            }
                        }
                        else
                        {
                            throw new MensajeException("No hay ubicaciones disponibles");
                        }
                    }
                }
                return ubicaciones;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
        public List<PosicionSatelital> obtenerUbicacionesVehiculosMudanza()
        {
            try
            {
                List<PosicionSatelital> ubicaciones = new List<PosicionSatelital>();
                if (Vehiculos.Count > 0)
                {
                    foreach (Vehiculo v in Vehiculos)
                    {
                        if (UbicacionVehiculos.Count > 0)
                        {
                            if (v.Tipo == TipoVehiculo.CamionMudanza)
                            {
                                string id = v.Id.ToString();
                                PosicionSatelital posicion =(PosicionSatelital) UbicacionVehiculos[id];
                                ubicaciones.Add(posicion);
                            }
                        }
                        else
                        {
                            throw new MensajeException("No hay ubicaciones disponibles");
                        }
                    }
                }
                return ubicaciones;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        //obtiene la ultima posicion de un determinado vehiculo
        public PosicionSatelital obtenerUltimaUbicacionVehiculo(string idVehiculo)
        {
            try
            {
                return (PosicionSatelital)UbicacionVehiculos[idVehiculo];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //hardcord de datos
        public async void cargarVehicuos()
        {
            var veh = await DBRepositoryMongo<Vehiculo>.GetItemsAsync("Vehiculos"); // CAMBIAR A RESPALDO VEHICULOS LUEGO DE TERMINAR PRUEBAS
            Vehiculos = veh.ToList();
            
        }
        public void datos()
        {
            //se harcodea hasta que se obtenga la posicion dinamicamente
            PosicionSatelital hard = new PosicionSatelital("5b724346011595287431869c", "-34.895249", "-56.126989");
            UbicacionVehiculos["5b724346011595287431869c"] = hard;
            PosicionSatelital hard2 = new PosicionSatelital("5b860426c55e96529884531e", "-34.909789", "-56.197760");
            UbicacionVehiculos["5b860426c55e96529884531e"] = hard2;
        }
        //se actualizan los datos del vehiculo en memoria cuando sufre algun cambio y se respaldan en una tabla accesoria de la base de datos ante cualquier inconveniente
        public async Task actualizarVehiculo(Vehiculo vehiculo)
        {
            try
            {
                Vehiculo eliminar = null;
                foreach(Vehiculo v in Vehiculos)
                {
                    if (v.Id.ToString().Equals(vehiculo.Id.ToString()))
                    {
                        eliminar = v;
                    }
                }
                Vehiculos.Remove(eliminar);
                Vehiculos.Add(vehiculo);
                var veh = await DBRepositoryMongo<Vehiculo>.GetItemAsync(vehiculo.Id.ToString(), "RespaldoVehiculos");
                if (veh != null)
                {
                    await DBRepositoryMongo<Vehiculo>.UpdateAsync(vehiculo.Id, vehiculo, "RespaldoVehiculos");
                }
                else
                {
                    await DBRepositoryMongo<Vehiculo>.Create(vehiculo, "RespaldoVehiculos");

                }
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

        public async Task actualizarTarifasVehiculos(Tarifa tarifa)
        {
            var vehiculos = await getVehiculos();
            List<Vehiculo> aux = vehiculos.ToList();
            foreach(Vehiculo v in aux)
            {
                if (v.Tipo == TipoVehiculo.Camioneta)
                {
                    v.Tarifa = tarifa.Camioneta;
                }
                if (v.Tipo == TipoVehiculo.CamionChico)
                {
                    v.Tarifa = tarifa.CamionChico;
                }
                if (v.Tipo == TipoVehiculo.Camion)
                {
                    v.Tarifa = tarifa.Camion;
                }
                if (v.Tipo == TipoVehiculo.CamionGrande)
                {
                    v.Tarifa = tarifa.CamionGrande;
                }
                if (v.Tipo == TipoVehiculo.CamionMudanza)
                {
                    v.Tarifa = tarifa.CamionMudanza;
                }
                await actualizarVehiculo(v);
            }
        }
        #endregion
    }
}
