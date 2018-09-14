using Microsoft.Extensions.Options;
using ProyectoTransportesAndes.Configuracion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProyectoTransportesAndes.Persistencia;
using System.Collections;
using ProyectoTransportesAndes.Exceptions;
using MongoDB.Bson;
using System.Net;
using System.Xml.Linq;

namespace ProyectoTransportesAndes.Models
{
    public class ControladoraViajes
    {
        #region "Atributos"
        private static ControladoraViajes _instancia;
        private IOptions<AppSettingsMongo> _settings;
        private Hashtable UbicacionesClientes { get; set; }
        //private List<Viaje> ServiciosPendientes { get; set; }
        #endregion

        #region "Propiedades"
        public static ControladoraViajes getInstancia(IOptions<AppSettingsMongo> settings)
        {
            if (_instancia == null)
            {
                _instancia = new ControladoraViajes(settings);
            }
            return _instancia;
        }
        #endregion

        #region "Constructores"
        private ControladoraViajes(IOptions<AppSettingsMongo> settings)
        {
            _settings = settings;
            UbicacionesClientes = new Hashtable();
            DBRepositoryMongo<Viaje>.Iniciar(_settings);
            DBRepositoryMongo<Cliente>.Iniciar(_settings);
            DBRepositoryMongo<Tarifa>.Iniciar(_settings);
            DBRepositoryMongo<LiquidacionChofer>.Iniciar(_settings);
            DBRepositoryMongo<Presupuesto>.Iniciar(_settings);
           
        }
        #endregion

        #region "Metodos"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idViaje"></param>
        /// <returns>Un viaje solicitado</returns>
        public async Task<Viaje> getViaje(string idViaje)
        {
            try
            {
                Viaje viaje = await DBRepositoryMongo<Viaje>.GetItemAsync(idViaje, "Viajes");
                return viaje;
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
        /// 
        /// </summary>
        /// <returns>Lista con la totalidad de viajes del sistema</returns>
        public async Task<List<Viaje>> getViajes()
        {
            try
            {
                var viajesOnline = await DBRepositoryMongo<Viaje>.GetItemsAsync("Viajes");
                List<Viaje> auxOnline = viajesOnline.ToList();
                var viajesDirectos = await DBRepositoryMongo<Viaje>.GetItemsAsync("ViajesDirectos");
                List<Viaje> auxDirecto = viajesDirectos.ToList();
                List<Viaje> salida = new List<Viaje>();
                Cliente auxCliente = null;
                Chofer auxChofer = null;
                foreach (Viaje v in auxOnline)
                {
                    auxCliente = v.Cliente.Desencriptar(v.Cliente);
                    auxChofer = v.Vehiculo.Chofer.Desencriptar(v.Vehiculo.Chofer);
                    v.Vehiculo.Chofer = auxChofer;
                    v.Cliente = auxCliente;
                    salida.Add(v);
                }
                foreach (Viaje v in auxDirecto)
                {
                    auxCliente = v.Cliente.Desencriptar(v.Cliente);
                    auxChofer = v.Vehiculo.Chofer.Desencriptar(v.Vehiculo.Chofer);
                    v.Vehiculo.Chofer = auxChofer;
                    v.Cliente = auxCliente;
                    salida.Add(v);
                    salida.Add(v);
                }
                return salida;
            }catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
            {
                throw ex;
            }
           
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idViaje"></param>
        /// <returns>Viaje solicitado pendiente</returns>
        public async Task<Viaje>getViajePendiente(string idViaje)
        {
            try
            {
                Viaje viaje = await DBRepositoryMongo<Viaje>.GetItemAsync(idViaje, "ViajesPendientes");
                return viaje;
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
        /// 
        /// </summary>
        /// <returns>Lista de viajes online</returns>
        public async Task<List<Viaje>> getViajesOnline()
        {
            try
            {
                var viajesOnline = await DBRepositoryMongo<Viaje>.GetItemsAsync("Viajes");

                return viajesOnline.ToList();
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
        /// 
        /// </summary>
        /// <returns>Lista de viajes directos</returns>
        public async Task<List<Viaje>> getViajesDirectos()
        {
            try
            {
                var viajes = await DBRepositoryMongo<Viaje>.GetItemsAsync("ViajesDirectos");
                return viajes.ToList();
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
        /// Incluye viajes online y directos
        /// </summary>
        /// <param name="idCliente"></param>
        /// <returns>Lista de viajes por cliente</returns>
        public async Task<List<Viaje>> getViajesCliente(string idCliente)
        {
            var viajesOnline = await DBRepositoryMongo<Viaje>.GetItemsAsync("Viajes");
            List<Viaje> auxOnline = viajesOnline.Where(v => v.Cliente.Id.ToString().Equals(idCliente)).ToList();
            var viajesDirectos = await DBRepositoryMongo<Viaje>.GetItemsAsync("ViajesDirectos");
            List<Viaje> auxDirecto = viajesDirectos.Where(v => v.Cliente.Id.ToString().Equals(idCliente)).ToList();
            List<Viaje> salida = new List<Viaje>();
            foreach (Viaje v in auxOnline)
            {
                salida.Add(v);
            }
            foreach (Viaje v in auxDirecto)
            {
                salida.Add(v);
            }
            return salida;
        }
        /// <summary>
        /// Incluye viajes online y directos que no se encuentren finalizados
        /// </summary>
        /// <param name="idChofer"></param>
        /// <returns>Lista de viajes activos por chofer</returns>
        public async Task<List<Viaje>> getViajesActivosChofer(string idChofer)
        {
            var viajesOnline = await DBRepositoryMongo<Viaje>.GetItemsAsync("Viajes");
            List<Viaje> auxOnline = viajesOnline.Where(v => v.Vehiculo.Chofer.Id.ToString().Equals(idChofer)).Where(v => !v.Estado.Equals(EstadoViaje.Finalizado)).ToList();
            var viajesDirectos = await DBRepositoryMongo<Viaje>.GetItemsAsync("ViajesDirectos");
            List<Viaje> auxDirecto = viajesDirectos.Where(v => v.Vehiculo.Chofer.Id.ToString().Equals(idChofer)).Where(v => !v.Estado.Equals(EstadoViaje.Finalizado)).ToList();
            List<Viaje> salida = new List<Viaje>();
            foreach (Viaje v in auxOnline)
            {
                salida.Add(v);
            }
            foreach (Viaje v in auxDirecto)
            {
                salida.Add(v);
            }
            return salida;

        }
        /// <summary>
        /// Todos los viajes de un chofer
        /// </summary>
        /// <param name="idChofer"></param>
        /// <returns>Lista de viajes por chofer</returns>
        public async Task<List<Viaje>> getViajesChofer(string idChofer)
        {
            var viajesOnline = await DBRepositoryMongo<Viaje>.GetItemsAsync("Viajes");
            List<Viaje> auxOnline = viajesOnline.Where(v => v.Vehiculo.Chofer.Id.ToString().Equals(idChofer)).ToList();
            var viajesDirectos = await DBRepositoryMongo<Viaje>.GetItemsAsync("ViajesDirectos");
            List<Viaje> auxDirecto = viajesDirectos.Where(v => v.Vehiculo.Chofer.Id.ToString().Equals(idChofer)).ToList();
            List<Viaje> salida = new List<Viaje>();
            foreach (Viaje v in auxOnline)
            {
                salida.Add(v);
            }
            foreach (Viaje v in auxDirecto)
            {
                salida.Add(v);
            }
            return salida;
        }





        public async Task<Viaje> solicitarViaje(Viaje viaje, TipoVehiculo tipoVehiculo)
        {
            try
            {
                viaje.Fecha = DateTime.Today.Date;
                viaje.Estado = EstadoViaje.Pendiente; // es el estado del viaje hasta que el chofer lo toma, o el chofer lo finaliza
                double unidadesTraslado = 0;
                double pesoTotal = 0;
                foreach (Item i in viaje.Items)
                {
                    unidadesTraslado += ControladoraVehiculos.getInstance(_settings).calcularUnidades((double)i.Alto,(double)i.Ancho,(double)i.Profundidad);
                    pesoTotal += (double)i.Peso;
                }
                PosicionSatelital posicionOrigen = null;
                if (!viaje.Compartido)
                {
                    posicionOrigen = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(viaje.DireccionOrigen);

                }
                else
                {
                    if (viaje.Items[0] != null)
                    {
                        posicionOrigen = new PosicionSatelital();
                        posicionOrigen.Latitud = viaje.Items[0].Origen.Latitud;
                        posicionOrigen.Longitud = viaje.Items[0].Destino.Longitud;
                    }
                }
                if (posicionOrigen != null)
                {
                    viaje.Origen = posicionOrigen;
                    viaje.Destino = posicionOrigen;
                    string latitudOrigen = posicionOrigen.Latitud;
                    string longitudOrigen = posicionOrigen.Longitud;
                    Vehiculo vehiculoDisponible = null;
                    if(tipoVehiculo == TipoVehiculo.CamionMudanza)
                    {
                        vehiculoDisponible = await ControladoraVehiculos.getInstance(_settings).mejorVehiculoMudanza(latitudOrigen, longitudOrigen);
                    }
                    else
                    {
                        vehiculoDisponible = await ControladoraVehiculos.getInstance(_settings).mejorVehiculoFlete(latitudOrigen, longitudOrigen, unidadesTraslado, pesoTotal);

                    }
                    if (vehiculoDisponible != null)
                    {
                        viaje.Vehiculo = vehiculoDisponible;
                        if (!viaje.Compartido)
                        {
                            viaje.Vehiculo.Disponible = false;
                        }
                        else
                        {
                            viaje.Vehiculo.CapacidadCargaKg -= pesoTotal;
                            viaje.Vehiculo.Unidades -= unidadesTraslado;
                        }
                        if (viaje.Items != null)
                        {
                            viaje.Vehiculo = agreagrItemsaVehiculo(viaje.Items, viaje.Vehiculo);
                        }
                        TimeZoneInfo timeZone = TimeZoneInfo.Local;
                        viaje.HoraInicio = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone).TimeOfDay;
                        viaje.DuracionEstimadaHastaCliente = await ControladoraVehiculos.getInstance(_settings).tiempoDemora(latitudOrigen, longitudOrigen, viaje.Vehiculo.PosicionSatelital.Latitud, viaje.Vehiculo.PosicionSatelital.Longitud);
                        if (!string.IsNullOrEmpty(viaje.DireccionDestino))
                        {
                            viaje.Destino = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(viaje.DireccionDestino);
                        }
                        viaje.DuracionEstimadaTotal = await ControladoraVehiculos.getInstance(_settings).tiempoDemoraTotal(viaje);
                        viaje.CantidadKm =(double)await ControladoraVehiculos.getInstance(_settings).cantidadKmAproximado(viaje);
                        ControladoraVehiculos.getInstance(_settings).actualizarVehiculo(viaje.Vehiculo); //refresco el estado del vehiculo en base y en memoria
                        Viaje aux = await DBRepositoryMongo<Viaje>.GetItemAsync(viaje.Id.ToString(), "ViajesPendientes");
                        if (aux != null)
                        {
                            await DBRepositoryMongo<Viaje>.UpdateAsync(viaje.Id, viaje, "ViajesPendientes");
                        }
                        else
                        {
                            viaje.Cliente = viaje.Cliente.Encriptar(viaje.Cliente);
                            await DBRepositoryMongo<Viaje>.Create(viaje, "ViajesPendientes");

                        }
                        return viaje;
                    }
                    else
                    {
                        viaje.Vehiculo = new Vehiculo();
                        return viaje;
                    }
                }
                else
                {
                    throw new MensajeException("No se ha podido determinar la ubicación del cliente. Intente de nuevo mas tarde.");
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
        public async Task presupuestoNuevo(Presupuesto nuevo)
        {
            try
            {
                await DBRepositoryMongo<Presupuesto>.Create(nuevo, "Presupuestos");
            }catch(MensajeException msg)
            {
                throw msg;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Viaje> confirmarViaje(string idViaje)
        {
            try
            {
                Viaje viaje = await DBRepositoryMongo<Viaje>.GetItemAsync(idViaje, "ViajesPendientes");
                viaje.ConfirmacionCliente = true;
                TimeZoneInfo timeZone = TimeZoneInfo.Local;
                viaje.FechaConfirmacionCliente = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone);
                await DBRepositoryMongo<Viaje>.Create(viaje, "Viajes");
                await DBRepositoryMongo<Viaje>.DeleteAsync(idViaje, "ViajesPendientes");
                return viaje;
            }catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
            {
                throw ex;
            }
        }
        public async Task<double> cancelarViaje(string idViaje)
        {
            try
            {
                double costo = 0;
                Viaje viaje = await DBRepositoryMongo<Viaje>.GetItemAsync(idViaje, "ViajesPendientes");
                if (viaje != null)
                {
                    costo = 0;
                    await DBRepositoryMongo<Viaje>.DeleteAsync(idViaje, "ViajesPendientes");
                    return costo;
                }
                else
                {
                    Viaje confirmado = await DBRepositoryMongo<Viaje>.GetItemAsync(idViaje, "Viajes");
                    if (confirmado != null)
                    {
                        if (confirmado.Estado == EstadoViaje.Pendiente)
                        {
                            costo = confirmado.Vehiculo.Tarifa *0.25;
                        }
                        if (confirmado.Estado == EstadoViaje.EnCurso)
                        {
                            costo = -1;
                        }
                        if (confirmado.Estado == EstadoViaje.Finalizado)
                        {
                            costo = -2;
                        }
                    }
                }
                return costo;
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
        // se utiliza en el caso de que el viaje ya estuviera confirmado
        public async Task confirmarCancelacion(string idViaje, double costoFinal)
        {
            try
            {
                if (!string.IsNullOrEmpty(idViaje))
                {
                    Viaje cancelado = await getViaje(idViaje);
                    cancelado.Estado = EstadoViaje.Finalizado;
                    cancelado.CostoFinal = costoFinal;
                    TimeZoneInfo timeZone = TimeZoneInfo.Local;
                    cancelado.HoraFin = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone).TimeOfDay;
                    string idVehiculo = cancelado.Vehiculo.Id.ToString();
                    foreach(Vehiculo v in ControladoraVehiculos.getInstance(_settings).Vehiculos)
                    {
                        if (v.Id.ToString().Equals(idVehiculo))
                        {
                            v.Disponible = true;
                            if (cancelado.Items.Count > 0)
                            {
                                foreach(Item i in cancelado.Items)
                                {
                                    Item eliminar = null;
                                    foreach(Item item in v.Items)
                                    {
                                        if (i.Id.ToString().Equals(item.Id.ToString())){
                                            eliminar = item;
                                        }
                                    }
                                    v.Items.Remove(eliminar);
                                }
                            }
                        }
                    }
                    await DBRepositoryMongo<Viaje>.UpdateAsync(cancelado.Id, cancelado, "Viajes");
                }
            }catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
            {
                throw ex;
            }
        }
        //el chofer llega a destino y cambia el estado del viaje a en curso
        public async Task<Viaje> levanteViaje(Viaje viaje)
        {
            try
            {
                Viaje actualizar = await getViaje(viaje.Id.ToString());
                actualizar.Estado = EstadoViaje.EnCurso;
                await actualizarViaje(actualizar);
                return actualizar;
            }catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
            {
                throw ex;
            }
           
        }
        public async Task actualizarViaje(Viaje viaje)
        {
            try
            {
                await DBRepositoryMongo<Viaje>.UpdateAsync(viaje.Id, viaje, "Viajes");
            }catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
            {
                throw ex;
            }
            
        }
        public async Task <Viaje> viajePendienteCliente(string cliente)
        {
            try
            {

                var viajes = await DBRepositoryMongo<Viaje>.GetItemsAsync("ViajesPendientes");
                Viaje viajePendiente = viajes.FirstOrDefault(v => v.Cliente.Id.ToString().Equals(cliente));
                if (viajePendiente != null && viajePendiente.Cliente != null)
                {
                    viajePendiente.Cliente.Desencriptar(viajePendiente.Cliente);
                }
                return viajePendiente;
    
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
        public async Task agregarItem(Viaje viaje, Item item)
        {
            try
            {
                Viaje aux = await viajePendienteCliente(viaje.Cliente.Id.ToString());
                if (aux == null)
                {
                    Viaje nuevo = new Viaje();
                    nuevo.Items = new List<Item>();
                    nuevo.Items.Add(item);
                    nuevo.Cliente = viaje.Cliente;
                    nuevo.Compartido = viaje.Compartido;
                    if (!string.IsNullOrEmpty(viaje.DireccionDestino))
                    {
                        nuevo.DireccionDestino = viaje.DireccionDestino;
                        nuevo.Destino = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(viaje.DireccionDestino);
                    }
                    else
                    {
                        nuevo.DireccionDestino = item.DireccionDestino;
                        nuevo.Destino = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(item.DireccionDestino);

                    }

                    if (!string.IsNullOrEmpty(viaje.DireccionOrigen))
                    {
                        if (viaje.Compartido)
                        {
                            nuevo.DireccionOrigen = item.DireccionOrigen;
                            nuevo.Origen = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(item.DireccionOrigen);
                        }
                        else
                        {
                            nuevo.DireccionOrigen = viaje.DireccionOrigen;
                            nuevo.Origen = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(item.DireccionOrigen);
                        }
                       
                    }
                    else
                    {
                        nuevo.DireccionOrigen = item.DireccionOrigen;
                        nuevo.Origen = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(item.DireccionOrigen);
                    }
                    if (nuevo.Items.Count > 0)
                    {
                        foreach(Item i in nuevo.Items)
                        {
                            i.Origen = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(i.DireccionOrigen);
                            i.Destino = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(i.DireccionDestino);
                        }
                    }
                    nuevo.Cliente = nuevo.Cliente.Encriptar(nuevo.Cliente);
                    await DBRepositoryMongo<Viaje>.Create(nuevo, "ViajesPendientes");
                   
                }
                else
                {
                    aux.Items.Add(item);
                    if (aux.Items.Count > 0)
                    {
                        foreach (Item i in aux.Items)
                        {
                            i.Origen = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(i.DireccionOrigen);
                            i.Destino = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(i.DireccionDestino);
                        }
                    }
                    await DBRepositoryMongo<Viaje>.UpdateAsync(aux.Id, aux, "ViajesPendientes");
                 
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
        public async Task<Viaje> corroborarDireccionesItems(Viaje viaje)
        {
            try
            {
                foreach(Item i in viaje.Items)
                {
                    if (string.IsNullOrEmpty(i.DireccionOrigen))
                    {
                        i.DireccionOrigen = viaje.DireccionOrigen;
                        i.Origen = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(i.DireccionOrigen);
                    }
                    else
                    {
                        i.Origen = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(i.DireccionOrigen);
                    }
                    if (string.IsNullOrEmpty(i.DireccionDestino))
                    {
                        i.DireccionDestino = viaje.DireccionDestino;
                        i.Destino = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(i.DireccionDestino);
                    }
                    else
                    {
                        i.Destino = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(i.DireccionDestino);
                    }
                }
                return viaje;
            }catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Item> itemParaEditar(string idCliente, string idItem)
        {
            try
            {
                Viaje viaje =  await viajePendienteCliente(idCliente);
                Item item = null;
                foreach (Item i in viaje.Items)
                {
                    if (i.Id.ToString().Equals(idItem))
                    {
                        item = i;
                    }
                }
                return item;
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
        public async Task editarItem(string idCliente, Item item)
        {
            try
            {
                Viaje viaje = await viajePendienteCliente(idCliente);
                foreach (Item i in viaje.Items)
                {
                    if (i.Id == item.Id)
                    {
                        i.Alto = item.Alto;
                        i.Ancho = item.Ancho;
                        i.Descripcion = item.Descripcion;
                        i.Imagen = item.Imagen;
                        i.Peso = item.Peso;
                        i.Profundidad = item.Profundidad;
                        i.Tipo = item.Tipo;
                    }
                }
                await DBRepositoryMongo<Viaje>.UpdateAsync(viaje.Id, viaje, "ViajesPendienes");
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
        public async Task nuevoViaje(string idVehiculo, string idCliente, string direccion, DateTime fecha, TimeSpan horaInicio, string comentarios)
        {
            try
            {
                Vehiculo vehiculo = null;
                Cliente cliente = null;
                if (idVehiculo != null && idCliente != null)
                {
                    vehiculo = ControladoraVehiculos.getInstance(_settings).getVehiculoMemoria(idVehiculo);
                    if (!vehiculo.Disponible)
                    {
                        throw new MensajeException("El vehiculo no se encuentra disponible");
                    }
                    cliente = await ControladoraUsuarios.getInstance(_settings).getCliente(idCliente);
                }
                else
                {
                    throw new MensajeException("Ha ocurrido un error inesperado. Intente de nuevo mas tarde");
                }
                Viaje nuevo = new Viaje();
                nuevo.Vehiculo = vehiculo;
                nuevo.Cliente = cliente;
                if (direccion != null)
                {
                    nuevo.DireccionDestino = direccion;
                }
                else
                {
                    nuevo.DireccionDestino = cliente.Direccion;
                }
                nuevo.HoraInicio = horaInicio;
                nuevo.Fecha = fecha;
                nuevo.Comentarios = comentarios;
                nuevo.Estado = EstadoViaje.EnCurso;
                await DBRepositoryMongo<Viaje>.Create(nuevo, "ViajesDirectos");
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
        public async Task modificarViaje(string id, Viaje viaje)
        {
            try
            {
                if (id != null && viaje != null)
                {
                    viaje.Id = new ObjectId(id);
                    await DBRepositoryMongo<Viaje>.UpdateAsync(viaje.Id, viaje, "Viajes");
                }
                else
                {
                    throw new MensajeException("Ha ocurrido un error inesperado, intente de nuevo mas tarde");
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
        public async Task eliminarViaje(string id, Viaje viaje)
        {
            try
            {
                if (id != null && viaje != null)
                {
                    //viaje.Id = new ObjectId(id);
                    await DBRepositoryMongo<Viaje>.DeleteAsync(id, "Viajes");
                }
                else
                {
                    throw new MensajeException("Ocurrió un error inesperado. Intente de nuevo mas tarde");
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
        public async Task<Viaje> finalizarViaje(Viaje viaje)
        {
            if (viaje != null)
            {
                viaje.Estado = EstadoViaje.Finalizado;
                viaje.HoraFin = DateTime.UtcNow.TimeOfDay;
                TimeSpan total = viaje.HoraFin.Subtract(viaje.HoraInicio);
                double costo = calcularPrecio(total, viaje.Vehiculo.Tarifa,viaje.Compartido);
                viaje.CostoFinal = costo;
                viaje.Estado = EstadoViaje.Finalizado;
                await DBRepositoryMongo<Viaje>.UpdateAsync(viaje.Id, viaje, "Viajes");
                viaje.Vehiculo.Disponible = true;
                if (viaje.Items.Count > 0) // me fijo si el viaje tiene items. si tiene es porque lo cancelo o finalizo inesperadamente. los saco tabien del vehiculo
                {
                    foreach(Item i in viaje.Items)
                    {
                        Item eliminar = null;
                        foreach(Item it in viaje.Vehiculo.Items)
                        {
                            if (it.Id.ToString().Equals(i.ToString()))
                            {
                                eliminar = it;
                            }
                        }
                        viaje.Vehiculo.Items.Remove(eliminar);
                    }
                }
                ControladoraVehiculos.getInstance(_settings).actualizarVehiculo(viaje.Vehiculo);
                //await ControladoraVehiculos.getInstance(_settings).editarVehiculo(viaje.Vehiculo, viaje.Vehiculo.Id.ToString(), viaje.Vehiculo.Chofer.Id.ToString(), viaje.Vehiculo.Tipo);
                return viaje;
            }
            return null;
        }
        public PosicionSatelital guardarUbicacionCliente(string idCliente, string latitud, string longitud)
        {
            PosicionSatelital posicion = new PosicionSatelital(idCliente, latitud, longitud);
            if (UbicacionesClientes.Contains(idCliente))
            {
                UbicacionesClientes[idCliente] = posicion;
            }
            else
            {
                UbicacionesClientes.Add(posicion.Id, posicion);
            }
            return posicion;
        }
        public PosicionSatelital obtenerUbicacionCliente(string idCliente)
        {
            return (PosicionSatelital)UbicacionesClientes[idCliente];
        }
        public double calcularPrecio(TimeSpan tiempo, int tarifa, bool compartido)
        {
            double precio = 0;
            if (tiempo.TotalMinutes > 60)
            {
                precio = (tiempo.TotalMinutes / 60) * tarifa;
            }
            else
            {
                precio = tarifa;
            }
            if (compartido)
            {
                precio = precio * 0.7;
            }
            return precio;
        }
        public Vehiculo agreagrItemsaVehiculo(List<Item> paraLlevar, Vehiculo vehiculo)
        {
            foreach (Item i in paraLlevar)
            {
                vehiculo.Items.Add(i);
            }
            return vehiculo; 
        }
        public async Task<Tarifa> obtenerUltimaTarifa()
        {
            try
            {
                var tarifas = await DBRepositoryMongo<Tarifa>.GetItemsAsync("Tarifas");
                Tarifa ultima = tarifas.OrderByDescending(t => t.FechaModificacion).FirstOrDefault();
                return ultima;
            }catch(MensajeException msg)
            {
                throw msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Tarifa> guardarTarifa(Tarifa nueva, string idUsuario)
        {
            try
            {
                Usuario admin = await ControladoraUsuarios.getInstance(_settings).getAdministrativo(idUsuario);
                TimeZoneInfo timeZone = TimeZoneInfo.Local;
                nueva.FechaModificacion = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone);
                nueva.UsuarioModificacion = admin.User;
                await DBRepositoryMongo<Tarifa>.Create(nueva, "Tarifas"); 
                return nueva;
            }catch(MensajeException msg)
            {
                throw msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<LiquidacionChofer>viajesParaLiquidarChofer(string idChofer, string idUsuario) 
        {
            try
            {
                LiquidacionChofer liquidacion = new LiquidacionChofer();
                Chofer chofer = await ControladoraUsuarios.getInstance(_settings).getChofer(idChofer);
                if (chofer != null)
                {
                    liquidacion.Chofer = chofer;
                    List<Viaje>paraLiquidar = await viajesParaLiquidarChofer(chofer);
                    liquidacion.Viajes = paraLiquidar;
                    liquidacion.FechaLiquidacion = DateTime.Now;
                    var administrativo = await ControladoraUsuarios.getInstance(_settings).getAdministrativo(idUsuario);
                    liquidacion.Administrativo = administrativo.User;
                }
               
                return liquidacion;
            }catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
            {
                throw ex;
            }
        }
        public async Task<LiquidacionChofer> liquidar(LiquidacionChofer liquidacion)
        {
            try
            {
                if (liquidacion != null)
                {
                    double totalViajes = 0, totalComision = 0, totalLiquidacion = 0;
                    foreach (Viaje v in liquidacion.Viajes)
                    {
                      
                            totalViajes += v.CostoFinal;
                            totalComision = totalViajes * 0.25;
                            totalLiquidacion = totalViajes - totalComision;
                        
                    }
                    liquidacion.TotalComision = totalComision;
                    liquidacion.Liquidacion = totalLiquidacion;
                    liquidacion.TotalViajes = totalViajes;
                }
                liquidacion.Pendiente = true;
                await DBRepositoryMongo<LiquidacionChofer>.Create(liquidacion, "Liquidaciones");
                return liquidacion;
            }catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<Viaje>>viajesParaLiquidarChofer(Chofer chofer)
        {
            try
            {
                List<Viaje> salida = new List<Viaje>();
                var viajes = await getViajes();
                salida = viajes.Where(v => !v.Liquidado).Where(v => v.Estado.Equals(EstadoViaje.Finalizado)).Where(v => v.Vehiculo.Chofer.Id.Equals(chofer.Id)).ToList();
                ////var viajesOnline =await DBRepositoryMongo<Viaje>.GetItemsAsync("Viajes");
                //var viajesOnline = await getViajesOnline();

                //List<Viaje> auxOnline = viajesOnline.Where(v=>!v.Liquidado).Where(v=>v.Estado.Equals(EstadoViaje.Finalizado)).Where(v=>v.Vehiculo.Chofer.Id.Equals(chofer.Id)).ToList();
                //var viajesDirectos = await DBRepositoryMongo<Viaje>.GetItemsAsync("ViajesDirectos");
                //List<Viaje> auxDirectos = viajesDirectos.Where(v =>!v.Liquidado).Where(v => v.Estado.Equals(EstadoViaje.Finalizado)).Where(v => v.Vehiculo.Chofer.Id.Equals(chofer.Id)).ToList();
                //List<Viaje> salida = new List<Viaje>();
                //foreach(Viaje v in auxOnline)
                //{
                //    salida.Add(v);
                //}
                //foreach (Viaje v in auxDirectos)
                //{
                //    salida.Add(v);
                //}
                return salida;
            }
            catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
            {
                throw ex;
            }
        }
        public async Task confirmarLiquidacion(string idLiquidacionChofer)
        {
            try
            {
                LiquidacionChofer liquidacion = await DBRepositoryMongo<LiquidacionChofer>.GetItemAsync(idLiquidacionChofer, "Liquidaciones");
                if (liquidacion != null)
                {
                    List<Viaje> directos = await getViajesDirectos();
                    List<Viaje> online = await getViajesOnline();
                    foreach(Viaje v in liquidacion.Viajes)
                    {
                        v.Liquidado = true;
                        if (directos.Contains(v))
                        {
                            v.Cliente = v.Cliente.Encriptar(v.Cliente);
                            v.Vehiculo.Chofer = v.Vehiculo.Chofer.Encriptar(v.Vehiculo.Chofer);
                            await DBRepositoryMongo<Viaje>.UpdateAsync(v.Id, v, "ViajesDirectos");
                        }
                        else
                        {
                            v.Vehiculo.Chofer = v.Vehiculo.Chofer.Encriptar(v.Vehiculo.Chofer);
                            v.Cliente = v.Cliente.Encriptar(v.Cliente);
                            await DBRepositoryMongo<Viaje>.UpdateAsync(v.Id, v, "Viajes");
                        }
                    }
                    liquidacion.Pendiente = false;
                    await DBRepositoryMongo<LiquidacionChofer>.UpdateAsync(liquidacion.Id, liquidacion, "Liquidaciones");
                }

                
            }catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
            {
                throw ex;
            }
        }
        public async Task cancelarLiquidacion(string idLiquidacion)
        {
            try
            {
                var liquidacion = await DBRepositoryMongo<LiquidacionChofer>.GetItemAsync(idLiquidacion, "Liquidaciones");
                await DBRepositoryMongo<LiquidacionChofer>.DeleteAsync(idLiquidacion, "Liquidaciones");
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
        public async Task<List<Presupuesto>> presupuestosPendientes()
        {
            try
            {
                var aux = await DBRepositoryMongo<Presupuesto>.GetItemsAsync("Presupuestos");
                List<Presupuesto> pendientes = aux.Where(p => p.Realizado == false).ToList();
                return pendientes;
            }catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
            {
                throw ex;
            }
        }
        public async Task confirmarPresupuesto(string idPresupuesto)
        {
            try
            {
                Presupuesto presupuesto = await DBRepositoryMongo<Presupuesto>.GetItemAsync(idPresupuesto,"Presupuestos");
                presupuesto.Realizado = true;
                await DBRepositoryMongo<Presupuesto>.UpdateAsync(presupuesto.Id, presupuesto, "Presupuestos");
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
        public async Task<List<LiquidacionChofer>> liquidacionesRealizadas()
        {
            try
            {
                var aux = await DBRepositoryMongo<LiquidacionChofer>.GetItemsAsync("Liquidaciones");
                List<LiquidacionChofer> liquidacionesRealizadas = aux.Where(l=>!l.Pendiente).ToList();
                return liquidacionesRealizadas;
            }catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
            {
                throw ex;
            }
        }
        public async Task<EstadisticaVehiculo>estadisticaVehiculo(string año, string mes, string idVehiculo)
        {
            try
            {
                var auxViajesOnline = await DBRepositoryMongo<Viaje>.GetItemsAsync("Viajes");
                var auxViajesDirectos = await DBRepositoryMongo<Viaje>.GetItemsAsync("ViajesDirectos");
                List<Viaje> viajesOnline = auxViajesOnline.Where(v=>v.Estado==EstadoViaje.Finalizado).Where(v=>v.Vehiculo.Id.ToString().Equals(idVehiculo)).ToList();
                List<Viaje> viajesDirectos = auxViajesDirectos.Where(v => v.Estado == EstadoViaje.Finalizado).Where(v => v.Vehiculo.Id.ToString().Equals(idVehiculo)).ToList();
                int cantidadViajesDirectos = 0;
                int cantidadViajesOnline = 0;
                double kmRecorridos = 0;
                double totalRecaudado = 0;
                double cantidadCombustible = 0;


                foreach (Viaje v in viajesOnline)
                {
                    if (!string.IsNullOrEmpty(mes))
                    {
                        if (v.Fecha.Year==int.Parse(año))
                        {
                            if (v.Fecha.Month==int.Parse(mes))
                            {
                                cantidadViajesOnline += 1;
                                kmRecorridos += v.CantidadKm;
                                totalRecaudado += v.CostoFinal;
                                cantidadCombustible += (v.CantidadKm / v.Vehiculo.ConsumoKml);
                            }
                        }
                    }
                    else
                    {
                        if (v.Fecha.Year==int.Parse(año))
                        {
                                cantidadViajesOnline += 1;
                                kmRecorridos += v.CantidadKm;
                                totalRecaudado += v.CostoFinal;
                                cantidadCombustible += (v.CantidadKm / v.Vehiculo.ConsumoKml);   
                        }
                    }
                   
                }
                foreach(Viaje v in viajesDirectos)
                {
                    if (!string.IsNullOrEmpty(mes))
                    {
                        if (v.Fecha.Year==int.Parse(año))
                        {
                            if (v.Fecha.Month==int.Parse(mes))
                            {
                                cantidadViajesOnline += 1;
                                kmRecorridos += v.CantidadKm;
                                totalRecaudado += v.CostoFinal;
                                cantidadCombustible += (v.CantidadKm / v.Vehiculo.ConsumoKml);
                            }
                        }
                    }
                    else
                    {
                        if (v.Fecha.Year==int.Parse(mes))
                        {
                            cantidadViajesOnline += 1;
                            kmRecorridos += v.CantidadKm;
                            totalRecaudado += v.CostoFinal;
                            cantidadCombustible += (v.CantidadKm / v.Vehiculo.ConsumoKml);
                        }
                    }
                }
                EstadisticaVehiculo estadistica = new EstadisticaVehiculo();
                estadistica.Año = TimeSpan.Parse(año);
                estadistica.Mes = TimeSpan.Parse(mes);
                estadistica.CantidadCombustible = cantidadCombustible;
                estadistica.CantViajesDirectos = cantidadViajesDirectos;
                estadistica.CantViajesOnline = cantidadViajesOnline;
                estadistica.KmRecorridos = kmRecorridos;
                estadistica.TotalRecaudado = totalRecaudado;
                return estadistica;
            }
            catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
