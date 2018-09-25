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
using System.Globalization;

namespace ProyectoTransportesAndes.Models
{
    public class ControladoraVehiculos
    {
        #region Atributos

        private IOptions<AppSettingsMongo> _settings;
        private static ControladoraVehiculos _instance;
        private ControladoraViajes _controladoraViajes;

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
        public Hashtable UbicacionVehiculos { get; set; }

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

        #region Get's
        /// <summary>
        /// se toma en cuenta los vehiculos de la bd
        /// se desencriptan los choferes antes de devolverlos
        /// </summary>
        /// <returns>lista de vehiculos que estan ingresados al sistema</returns>
        public async Task<IEnumerable<Vehiculo>> getVehiculos()
        {
            try
            {
                var items = await DBRepositoryMongo<Vehiculo>.GetItemsAsync("Vehiculos");
                List<Vehiculo> salida = new List<Vehiculo>();
                foreach (Vehiculo v in items)
                {
                    if (!v.Chofer.Id.ToString().Equals("000000000000000000000000"))
                    {
                        Chofer c = v.Chofer;
                        v.Chofer = c.Desencriptar(c);
                        salida.Add(v);
                    }
                    else
                    {
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

        /// <summary>
        /// se toma en cuenta la lista en memoria que  es la que lleva el control en tiempo real del estado del vehiculo
        /// </summary>
        /// <returns>lista de vehiculos disponibles</returns>
        public List<Vehiculo> getVehiculosDisponibles()
        {
            try
            {
                List<Vehiculo> vehiculosDisponibles = Vehiculos.Where(v => v.Disponible == true).Where(v => v.Items.Count == 0).ToList();
                return vehiculosDisponibles;
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

        /// <summary>
        /// se toma en cuenta la lista en memoria. este metodo se utiliza para la adjudicación de viajes
        /// </summary>
        /// <param name="idVehiculo"></param>
        /// <returns>vehiculo solicitado</returns>
        public Vehiculo getVehiculoMemoria(string idVehiculo)
        {
            try
            {
                if (idVehiculo != null)
                {
                    Vehiculo vehiculo = null;
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

        /// <summary>
        /// se toma en cuenta los vehiculos de la bd. se utiliza para el abm
        /// </summary>
        /// <param name="idVehiculo"></param>
        /// <returns>vehiculo solicitado</returns>
        public async Task<Vehiculo> getVehiculoBaseDatos(string idVehiculo)
        {
            try
            {
                Vehiculo vehiculo = await DBRepositoryMongo<Vehiculo>.GetItemAsync(idVehiculo, "Vehiculos");
                return vehiculo;
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

        /// <summary>
        /// se devuelve el chofer con el id seleccionado
        /// se desencripta antes de devolverlo
        /// </summary>
        /// <param name="idChofer"></param>
        /// <returns>Chofer</returns>
        public async Task<Chofer> getChofer(string idChofer)
        {
            try
            {
                if (idChofer != null)
                {
                    Chofer chofer = await DBRepositoryMongo<Chofer>.GetItemAsync(idChofer, "Choferes");
                    chofer = chofer.Desencriptar(chofer);
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

        /// <summary>
        /// se devuelve el vehiculo que tiene un chofer determinado desde la bd
        /// </summary>
        /// <param name="idChofer"></param>
        /// <returns></returns>
        public async Task<Vehiculo> getVehiculoChofer(string idChofer)
        {
            try
            {
                var aux = await DBRepositoryMongo<Vehiculo>.GetItemsAsync("Vehiculos");
                Vehiculo vehiculo = aux.Where(v => v.Chofer.Id.ToString().Equals(idChofer)).FirstOrDefault();
                return vehiculo;
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

        #endregion

        #region Abm

        /// <summary>
        /// guarda un nuevo vehiculo con o sin chofer. si no tiene chofer no queda disponible
        /// </summary>
        /// <param name="vehiculo"></param>
        /// <param name="idChofer"></param>
        /// <returns></returns>
        public async Task nuevoVehiculo(Vehiculo vehiculo, string idChofer)
        {
            try
            {
                if (vehiculo != null && !idChofer.Equals("000000000000000000000000"))
                {
                    Chofer chofer = await getChofer(idChofer);
                    chofer.Disponible = false;
                    chofer = chofer.Encriptar(chofer);
                    vehiculo.Chofer = chofer;
                    vehiculo.Disponible = true;
                    vehiculo.Unidades = calcularUnidades(vehiculo.Largo, vehiculo.Ancho, vehiculo.Alto);
                    Tarifa t = await ControladoraViajes.getInstancia(_settings).obtenerUltimaTarifa();
                    if (vehiculo.Tipo.Equals(TipoVehiculo.Camioneta))
                    {
                        vehiculo.Tarifa = t.Camioneta;
                    }
                    if (vehiculo.Tipo.Equals(TipoVehiculo.CamionChico))
                    {
                        vehiculo.Tarifa = t.CamionChico;
                    }
                    if (vehiculo.Tipo.Equals(TipoVehiculo.Camion))
                    {
                        vehiculo.Tarifa = t.Camion;
                    }
                    if (vehiculo.Tipo.Equals(TipoVehiculo.CamionGrande))
                    {
                        vehiculo.Tarifa = t.CamionGrande;
                    }
                    if (vehiculo.Tipo.Equals(TipoVehiculo.CamionMudanza))
                    {
                        vehiculo.Tarifa = t.CamionMudanza;
                    }
                    await DBRepositoryMongo<Vehiculo>.Create(vehiculo, "Vehiculos");
                    Vehiculos.Add(vehiculo);
                    await DBRepositoryMongo<Chofer>.UpdateAsync(chofer.Id, chofer, "Choferes");
                }
                else if (vehiculo != null && idChofer.Equals("000000000000000000000000"))
                {
                    Tarifa t = await ControladoraViajes.getInstancia(_settings).obtenerUltimaTarifa();

                    if (vehiculo.Tipo.Equals(TipoVehiculo.Camioneta))
                    {
                        vehiculo.Tarifa = t.Camioneta;
                    }
                    if (vehiculo.Tipo.Equals(TipoVehiculo.CamionChico))
                    {
                        vehiculo.Tarifa = t.CamionChico;
                    }
                    if (vehiculo.Tipo.Equals(TipoVehiculo.Camion))
                    {
                        vehiculo.Tarifa = t.Camion;
                    }
                    if (vehiculo.Tipo.Equals(TipoVehiculo.CamionGrande))
                    {
                        vehiculo.Tarifa = t.CamionGrande;
                    }
                    if (vehiculo.Tipo.Equals(TipoVehiculo.CamionMudanza))
                    {
                        vehiculo.Tarifa = t.CamionMudanza;
                    }
                    vehiculo.Chofer = new Chofer();
                    vehiculo.Disponible = false;
                    vehiculo.Unidades = calcularUnidades(vehiculo.Largo, vehiculo.Ancho, vehiculo.Alto);
                    await DBRepositoryMongo<Vehiculo>.Create(vehiculo, "Vehiculos");
                    Vehiculos.Add(vehiculo);
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
        /// <summary>
        /// Se edita el vehiculo pasado. El id del vehiculo se utiliza para reconstruir el objeto ObjectId.
        /// si no se pasa un chofer, se crea solo un vehiculo
        /// </summary>
        /// <param name="vehiculo"></param>
        /// <param name="idVehiculo"></param>
        /// <param name="choferSeleccionado"></param>
        /// <param name="tipoVehiculo"></param>
        /// <returns></returns>
        public async Task editarVehiculo(Vehiculo vehiculo, string idVehiculo, string choferSeleccionado, TipoVehiculo tipoVehiculo)
        {
            try
            {
                if (vehiculo != null && !choferSeleccionado.Equals("000000000000000000000000"))
                {
                    vehiculo.Id = new ObjectId(idVehiculo);
                    vehiculo.Unidades = calcularUnidades(vehiculo.Alto, vehiculo.Ancho, vehiculo.Largo);
                    vehiculo.Tipo = tipoVehiculo;
                    //Chofer chofer = await DBRepositoryMongo<Chofer>.GetItemAsync(choferSeleccionado, "Choferes");
                    Chofer chofer = await ControladoraUsuarios.getInstance(_settings).getChofer(choferSeleccionado);
                    if (chofer != null)
                    {
                        vehiculo.Chofer = chofer;
                    }
                    chofer = chofer.Encriptar(chofer);
                    await DBRepositoryMongo<Vehiculo>.UpdateAsync(vehiculo.Id, vehiculo, "Vehiculos");
                    Vehiculo eliminar = null;
                    foreach (Vehiculo v in Vehiculos)
                    {
                        if (v.Id.ToString().Equals(idVehiculo))
                        {
                            if (v.Disponible && v.Items.Count > 0)// me fijo que no este en un viaje
                            {
                                eliminar = v;
                            }
                        }
                    }
                    if (eliminar != null)
                    {
                        Vehiculos.Remove(eliminar);
                        Vehiculos.Add(vehiculo);
                    }
                }
                else if (vehiculo != null && choferSeleccionado.Equals("000000000000000000000000"))
                {
                    vehiculo.Id = new ObjectId(idVehiculo);
                    vehiculo.Unidades = calcularUnidades(vehiculo.Alto, vehiculo.Ancho, vehiculo.Largo);
                    vehiculo.Tipo = tipoVehiculo;
                    vehiculo.Chofer = new Chofer();
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

        /// <summary>
        /// se elimina el vehiculo y se actualiza el estado del chofer.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="vehiculo"></param>
        /// <returns></returns>
        public async Task eliminarVehiculo(string id)
        {
            try
            {
                if (id != null)
                {
                    Vehiculo eliminar = await DBRepositoryMongo<Vehiculo>.GetItemAsync(id, "Vehiculos");
                    await DBRepositoryMongo<Vehiculo>.DeleteAsync(id, "Vehiculos");
                    Vehiculos.Remove(eliminar);
                    eliminar.Chofer.Disponible = true;
                    await DBRepositoryMongo<Chofer>.UpdateAsync(eliminar.Chofer.Id, eliminar.Chofer, "Choferes");
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

        #endregion

        #region Utilidades

        /// <summary>
        /// se actualiza el estado en memoria del vehiculo pasado como parametro
        /// </summary>
        /// <param name="vehiculo"></param>
        public void actualizarVehiculo(Vehiculo vehiculo)
        {
            try
            {
                Vehiculo eliminar = null;
                foreach (Vehiculo v in Vehiculos)
                {
                    if (v.Id.ToString().Equals(vehiculo.Id.ToString()))
                    {
                        eliminar = v;
                    }
                }
                Vehiculos.Remove(eliminar);
                Vehiculos.Add(vehiculo);
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

        /// <summary>
        /// actualiza las tarifas
        /// </summary>
        /// <param name="tarifa"></param>
        /// <returns></returns>
        public async Task actualizarTarifasVehiculos(Tarifa tarifa)
        {
            var vehiculos = await getVehiculos();
            List<Vehiculo> aux = vehiculos.ToList();
            foreach (Vehiculo v in aux)
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
                v.Chofer = v.Chofer.Encriptar(v.Chofer);
                await DBRepositoryMongo<Vehiculo>.UpdateAsync(v.Id, v, "Vehiculos");
                foreach (Vehiculo veh in Vehiculos)
                {
                    if (veh.Id.ToString().Equals(v.ToString()))
                    {
                        veh.Tarifa = v.Tarifa;
                    }
                }
            }
        }

        /// <summary>
        /// Es una aproximacion para determinar la capacidad de carga de un vehículo.
        /// Se toman las unidades como m3 de capacidad de carga.
        /// El peso de la carga se ira descontando de la capacidad del vehiculo.
        /// </summary>
        /// <param name="alto"></param>
        /// <param name="ancho"></param>
        /// <param name="profundidad"></param>
        /// <returns></returns>
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

        /// <summary>
        /// De acuerdo a la latitud y longitud del cliente se busca el vehiculo para mudanza mas cercano
        /// </summary>
        /// <param name="latitudCliente"></param>
        /// <param name="longitudCliente"></param>
        /// <returns>Retorna el vehiculo mas cercano para mudanza</returns>
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
                        vehiculo = getVehiculoMemoria(pos.Id);
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
                if (masCercano != null)
                {
                    masCercano.Chofer = masCercano.Chofer.Desencriptar(masCercano.Chofer);
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

        /// <summary>
        /// Retorna el vehiculo mas cercano al cliente con capacidad para trasladar lo necesario
        /// </summary>
        /// <param name="latitudCliente"></param>
        /// <param name="longitudCliente"></param>
        /// <param name="unidadadesTraslado"></param>
        /// <param name="pesoTraslado"></param>
        /// <returns></returns>
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
                        vehiculo = getVehiculoMemoria(pos.Id);
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
                if (masCercanoConCapacidad != null)
                {
                    masCercanoConCapacidad.Chofer = masCercanoConCapacidad.Chofer.Desencriptar(masCercanoConCapacidad.Chofer);
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

        /// <summary>
        /// determina el tiempo de demora entre dos juegos de coordenadas
        /// se utiliza para determinar el tiempo de demora entre el vehiculo y el cliente
        /// </summary>
        /// <param name="latitudCliente"></param>
        /// <param name="longitudCliente"></param>
        /// <param name="latitudVehiculo"></param>
        /// <param name="longitudVehiculo"></param>
        /// <returns>tiempo en minutos</returns>
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
                            if (!e.status.Equals("ZERO_RESULTS"))
                            {
                                tiempo = int.Parse(e.duration.text.Split(" ")[0]);
                            }
                            else
                            {
                                throw new MensajeException("Compruebe el formato de la dirección ingresada");
                            }
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
        
        /// <summary>
        /// calcula el tiempo de demora de un determinado viaje
        /// </summary>
        /// <param name="viaje"></param>
        /// <returns>tiempo en minutos</returns>
        public async Task<TimeSpan> tiempoDemoraTotal(Viaje viaje)
        {
            try
            {
                HttpClient cliente = new HttpClient();
                int tiempo = int.MaxValue;
                string path = null;
                if (!string.IsNullOrEmpty(viaje.DireccionDestino))
                {
                    path = "https://maps.googleapis.com/maps/api/directions/json?origin=" + viaje.Origen.Latitud + ", " + viaje.Origen.Longitud + "&destination=" + viaje.Destino.Latitud + "," + viaje.Destino.Longitud;
                }
                else
                {
                    path = "https://maps.googleapis.com/maps/api/directions/json?origin=" + viaje.Origen.Latitud + ", " + viaje.Origen.Longitud + "&destination=" + viaje.Origen.Latitud + "," + viaje.Origen.Longitud;

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
            catch (Exception)
            {
                throw new MensajeException("Compruebe el formato de las direcciones ingresadas");
            }
        }

        /// <summary>
        /// basandose en la api de google se calculan aprox los km del viaje
        /// </summary>
        /// <param name="viaje"></param>
        /// <returns>double con la cantidad de km del viaje</returns>
        public async Task<double> cantidadKmAproximado(Viaje viaje)
        {
            try
            {
                HttpClient cliente = new HttpClient();
                double salida = 0;
                string path = null;
                if (!string.IsNullOrEmpty(viaje.DireccionDestino))
                {
                    path = "https://maps.googleapis.com/maps/api/directions/json?origin=" + viaje.Origen.Latitud + ", " + viaje.Origen.Longitud + "&destination=" + viaje.Destino.Latitud + "," + viaje.Destino.Longitud;
                }
                else
                {
                    path = "https://maps.googleapis.com/maps/api/directions/json?origin=" + viaje.Origen.Latitud + ", " + viaje.Origen.Longitud + "&destination=" + viaje.Vehiculo.PosicionSatelital.Latitud+ "," + viaje.Vehiculo.PosicionSatelital.Longitud;

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
                    if (leg.distance.text.Split(" ")[1].Equals("km"))
                    {
                        double km =leg.distance.value;
                        salida = km / 1000;
                        
                    }
                }

                return salida;
            }
            catch (TimeoutException)
            {
                throw new MensajeException("Se agoto el tiempo de espera, vuelva a intentarlo en unos minutos");
            }
            catch (MensajeException msg)
            {
                throw msg;
            }catch(Exception)
            {
                throw new MensajeException("Compruebe el formato de las direcciones ingresadas");
            }
        }
            
        /// <summary>
        /// convierte una direccion de puerta en un juego de coordenadas geográficas
        /// </summary>
        /// <param name="direccion"></param>
        /// <returns></returns>
        public async Task<PosicionSatelital> convertirDireccionEnCoordenadas(string direccion)
        {
            try
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
            catch (TimeoutException)
            {
                throw new MensajeException("Se agoto el tiempo de espera, vuelva a intentarlo en unos minutos");
            }
            catch (Exception)
            {
                throw new MensajeException("Compruebe el formato de las direcciones ingresadas");
            }
           
        }

        /// <summary>
        /// convierte un par de coordenadas en una direccion de calle
        /// </summary>
        /// <param name="latitud"></param>
        /// <param name="longitud"></param>
        /// <returns></returns>
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

        /// <summary>
        /// devuelve una lista de vehiculos con la posicion satelital actualizada segun el hash de posiciones que hay en memoria
        /// </summary>
        /// <returns>lista de vehiculos</returns>
        public async Task<List<Vehiculo>> vehiculosConPosicion()
        {
            try
            {
                List<PosicionSatelital> posicionesFletes = obtenerUbicacionesVehiculosFletes();
                List<PosicionSatelital> posicionesMudanza = obtenerUbicacionesVehiculosMudanza();
                List<PosicionSatelital> aux = new List<PosicionSatelital>();
                foreach (PosicionSatelital p in posicionesFletes)
                {
                    aux.Add(p);
                }
                foreach (PosicionSatelital p in posicionesMudanza)
                {
                    aux.Add(p);
                }
                List<Vehiculo> salida = new List<Vehiculo>();
                if (posicionesFletes != null && aux.Count > 0)
                {
                    foreach (PosicionSatelital pos in aux)
                    {
                        Vehiculo v = await DBRepositoryMongo<Vehiculo>.GetItemAsync(pos.Id, "Vehiculos");
                        if (v != null)
                        {
                            v.PosicionSatelital = pos;
                            salida.Add(v);
                        }

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

        /// <summary>
        /// devuelve la lista de choferes de la bd que no tienen vehiculos asociados
        /// </summary>
        /// <returns>lista de choferes</returns>
        public async Task<List<Chofer>> choferesDisponibles()
        {
            try
            {
                List<Chofer> salida = new List<Chofer>();
                var lista = await DBRepositoryMongo<Chofer>.GetItemsAsync("Choferes");
                List<Chofer> aux = new List<Chofer>();

                foreach (Chofer c in lista)
                {
                    Chofer chofer = c.Desencriptar(c);
                    aux.Add(chofer);
                }

                foreach (Chofer c in aux)
                {
                    if (c.Disponible)
                    {
                        salida.Add(c);
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

        /// <summary>
        /// desde la api guarda la posicion actual del vehiculo en el hash de posiciones
        /// </summary>
        /// <param name="idVehiculo"></param>
        /// <param name="latitud"></param>
        /// <param name="longitud"></param>
        /// <returns></returns>
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

        /// <summary>
        /// devuelve una lista de posiciones satelitales de los vehiculos aptos para fletes que hay en el hash de memoria
        /// </summary>
        /// <returns>lista de posiciones satelitales</returns>
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
                                if (UbicacionVehiculos[v.Id.ToString()] != null)
                                {
                                    ubicaciones.Add((PosicionSatelital)UbicacionVehiculos[v.Id.ToString()]);
                                }
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

        /// <summary>
        /// devuelve una lista de posiciones satelitales de los vehiculos aptos para mudanzas que hay en el hash de memoria
        /// </summary>
        /// <returns>lista de posiciones satelitales</returns>
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
                                if (UbicacionVehiculos[v.Id.ToString()] != null)
                                {
                                    string id = v.Id.ToString();
                                    PosicionSatelital posicion = (PosicionSatelital)UbicacionVehiculos[id];
                                    ubicaciones.Add(posicion);
                                }
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

        /// <summary>
        /// obtiene la ultima posicion conocida de un vehiculo que se encuentra en el hash en memoria
        /// </summary>
        /// <param name="idVehiculo"></param>
        /// <returns>posicion satelital</returns>
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

        /// <summary>
        /// la primera vez que inicia el sitema carga todos los vehiculos que hay en memoria
        /// </summary>
        public async void cargarVehicuos()
        {
            var veh = await DBRepositoryMongo<Vehiculo>.GetItemsAsync("Vehiculos");
            Vehiculos = veh.ToList();
        }

        #endregion

        #endregion



        public void datos()
        {
            //se harcodea hasta que se obtenga la posicion dinamicamente
            //PosicionSatelital hard = new PosicionSatelital("5b9a6bbefe2a714978444832", "-34.895249", "-56.126989");
            //UbicacionVehiculos["5b9a6bbefe2a714978444832"] = hard;
            PosicionSatelital hard2 = new PosicionSatelital("5b9a6bf2fe2a714978444833", "-34.909789", "-56.197760");
            UbicacionVehiculos["5b9a6bf2fe2a714978444833"] = hard2;
            PosicionSatelital hard3 = new PosicionSatelital("5b9b1b02c7de353ce4e553d9", "-34.895090", "-56.164818");
            UbicacionVehiculos["5b9b1b02c7de353ce4e553d9"] = hard3;
        }
    }
}
