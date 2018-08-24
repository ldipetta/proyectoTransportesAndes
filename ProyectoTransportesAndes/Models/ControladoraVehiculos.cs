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
        #endregion

        #region Constructores
        private ControladoraVehiculos(IOptions<AppSettingsMongo> settings)
        {
            _settings = settings;
            _controladoraViajes = ControladoraViajes.getInstancia(_settings);
            UbicacionVehiculos = new Hashtable();
            DBRepositoryMongo<Vehiculo>.Iniciar(_settings);
            DBRepositoryMongo<Chofer>.Iniciar(_settings);
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
        }catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
            {
                throw ex;
            }
          
        }
        public async Task<Vehiculo> getVehiculo(string idVehiculo)
        {
            try
            {
                if (idVehiculo != null)
                {
                    Vehiculo vehiculo = await DBRepositoryMongo<Vehiculo>.GetItemAsync(idVehiculo, "Vehiculos");
                    return vehiculo;
                }
                else
                {
                    throw new MensajeException("El Id del vehiculo no existe");
                }
            }catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
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
            catch(MensajeException msg)
            {
                throw msg;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }
        public async Task nuevoVehiculo(Vehiculo vehiculo, string idChofer)
        {
            try
            {
                if(vehiculo!=null && idChofer != null)
                {
                    Vehiculo nuevo = vehiculo;
                    Chofer chofer = await getChofer(idChofer);
                    chofer.Disponible = false;
                    nuevo.Chofer = chofer;
                    nuevo.Disponible = true;
                    nuevo.Unidades = calcularUnidades(nuevo.Largo, nuevo.Ancho, nuevo.Alto);
                    await DBRepositoryMongo<Vehiculo>.Create(nuevo, "Vehiculos");
                }
                else
                {
                    throw new MensajeException("Ha ocurrido un error inesperado. Vuelva a intentarlo mas tarde");
                }
               
            }
            catch(MensajeException msg)
            {
                throw msg;
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
        public async Task editarVehiculo(Vehiculo vehiculo, string idVehiculo)
        {
            try
            {
                if(vehiculo!=null && idVehiculo != null)
                {
                    vehiculo.Id = new ObjectId(idVehiculo);
                    vehiculo.Unidades = calcularUnidades(vehiculo.Alto, vehiculo.Ancho, vehiculo.Largo);
                    await DBRepositoryMongo<Vehiculo>.UpdateAsync(vehiculo.Id, vehiculo, "Vehiculos");
                }
                else
                {
                    throw new MensajeException("Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                }
                
            }
            catch(MensajeException msg)
            {
                throw msg;
            }
            catch(Exception ex)
            {
                throw ex;
            }
           
        }
        public async Task eliminarVehiculo(string id, Vehiculo vehiculo)
        {
            try
            {
                if(vehiculo!=null && id != null)
                {
                    vehiculo.Id = new ObjectId(id);
                    await DBRepositoryMongo<Vehiculo>.DeleteAsync(vehiculo.Id, "Vehiculos");
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
            }catch(Exception ex)
            {
                throw ex;
            }
            
        }

        //public async Task<Vehiculo> mejorVehiculo(string latitudCliente, string longitudCliente, double unidadadesTraslado, double pesoTraslado)
        //{
        //    try
        //    {
        //        HttpClient cliente = new HttpClient();
        //        List<Vehiculo> aux = new List<Vehiculo>();
        //        List<PosicionSatelital> ubicaciones = obtenerUbicacionesVehiculos();
        //        int menorTiempo = int.MaxValue;
        //        Vehiculo vehiculo = null;
        //        Vehiculo masCercanoConCapacidad = null;
        //        foreach (PosicionSatelital pos in ubicaciones)
        //        {
        //            int tiempo = int.MaxValue;
        //            string path = "https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=" + latitudCliente + "," + longitudCliente + "&destinations=" + pos.Latitud + "," + pos.Longitud + "&key=AIzaSyB08YiU7GpCk0RCQozZWxiIj3Ud3se0_Ec";
        //            HttpResponseMessage response = await cliente.GetAsync(path);
        //            if (response.IsSuccessStatusCode)
        //            {
        //                RootObject obj = JsonConvert.DeserializeObject<RootObject>(await response.Content.ReadAsStringAsync());
        //                foreach (Row r in obj.rows)
        //                {
        //                    foreach (Element e in r.elements)
        //                    {
        //                        tiempo = int.Parse(e.duration.text.Split(" ")[0]);
        //                        if (tiempo < menorTiempo)
        //                        {
        //                            menorTiempo = tiempo;
        //                            vehiculo = await DBRepositoryMongo<Vehiculo>.GetItemAsync(pos.Id, "Vehiculos");
        //                            vehiculo.PosicionSatelital = obtenerUltimaUbicacionVehiculo(vehiculo.Id.ToString());
        //                            if (vehiculo.Disponible)
        //                            {
        //                                if (masCercanoConCapacidad == null)
        //                                {
        //                                    if (vehiculo.Unidades >= unidadadesTraslado)
        //                                    {
        //                                        if (vehiculo.CapacidadCargaKg >= pesoTraslado)
        //                                        {
        //                                            masCercanoConCapacidad = vehiculo;
        //                                        }
        //                                    }

        //                                }
        //                                else
        //                                {
        //                                    if (vehiculo.Unidades < masCercanoConCapacidad.Unidades && vehiculo.Unidades >= unidadadesTraslado)
        //                                    {
        //                                        if (vehiculo.CapacidadCargaKg < masCercanoConCapacidad.CapacidadCargaKg && vehiculo.CapacidadCargaKg >= pesoTraslado)
        //                                        {
        //                                            masCercanoConCapacidad = vehiculo;
        //                                        }
        //                                    }
        //                                }
        //                            }

        //                        }
        //                    }
        //                }
        //            }

        //        }
        //        return masCercanoConCapacidad;
        //    }
        //    catch (TimeoutException)
        //    {
        //        throw new MensajeException("Se agoto el tiempo de espera. Compruebe la conexion");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        // devuelve el vehiculo mas cercano al cliente con capacidad de carga para trasladar lo necesario
        public async Task<Vehiculo> mejorVehiculoPrueba(string latitudCliente, string longitudCliente, double unidadadesTraslado, double pesoTraslado)
        {
            try
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
                        else
                        {
                            throw new MensajeException("El vehiculo no se encuentra disponible");
                        }

                    }
                }
                return masCercanoConCapacidad;
            }
            catch (TimeoutException)
            {
                throw new MensajeException("Se agoto el tiempo de espera. Compruebe la conexion");
            }
            catch(MensajeException msg)
            {
                throw msg;
            }
            catch(Exception ex)
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
        //devuelve la lista de vehiculos que hay en el sistema con su posicion asociada
        public async Task<List<Vehiculo>> vehiculosConPosicion()
        {
            try
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
            }catch(MensajeException msg)
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
            catch(MensajeException msg)
            {
                throw msg;
            }
            catch(Exception ex)
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
            }catch(Exception ex)
            {
                throw ex;
            }
            
        }
        //devuelve una lista de las posiciones de los vehiculos obtenidas del hash en memoria
        public List<PosicionSatelital> obtenerUbicacionesVehiculos()
        {
            try
            {
                List<PosicionSatelital> ubicaciones = new List<PosicionSatelital>();
                if (UbicacionVehiculos.Count > 0)
                {
                    foreach (DictionaryEntry d in UbicacionVehiculos)
                    {
                        ubicaciones.Add((PosicionSatelital)d.Value);
                    }
                }
                else
                {
                    throw new MensajeException("No hay ubicaciones disponibles");
                }
                return ubicaciones;
            }catch(Exception ex)
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
           catch(Exception ex)
            {
                throw ex;
            }
        }
        //hardcord de datos
        public void datos()
        {
            PosicionSatelital hard = new PosicionSatelital("5b60ad9ab73c94313c6c7552", "-34.895249", "-56.126989");
            UbicacionVehiculos["5b60ad9ab73c94313c6c7552"] = hard;
        }
        #endregion
    }
}
