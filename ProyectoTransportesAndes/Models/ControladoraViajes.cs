using Microsoft.Extensions.Options;
using ProyectoTransportesAndes.Configuracion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProyectoTransportesAndes.Persistencia;
using ProyectoTransportesAndes.Exceptions;
using MongoDB.Bson;

namespace ProyectoTransportesAndes.Models
{
    public class ControladoraViajes
    {
        #region Atributos

        private static ControladoraViajes _instancia;
        private IOptions<AppSettingsMongo> _settings;

        #endregion

        #region Propiedades

        public static ControladoraViajes getInstancia(IOptions<AppSettingsMongo> settings)
        {
            if (_instancia == null)
            {
                _instancia = new ControladoraViajes(settings);
            }
            return _instancia;
        }

        #endregion

        #region Constructores

        private ControladoraViajes(IOptions<AppSettingsMongo> settings)
        {
            _settings = settings;
            DBRepositoryMongo<Viaje>.Iniciar(_settings);
            DBRepositoryMongo<Cliente>.Iniciar(_settings);
            DBRepositoryMongo<Tarifa>.Iniciar(_settings);
            DBRepositoryMongo<LiquidacionChofer>.Iniciar(_settings);
            DBRepositoryMongo<Presupuesto>.Iniciar(_settings);

        }

        #endregion

        #region Metodos

        #region Get's

        /// <summary>
        /// devuelve el viaje con el id seleccionado
        /// se desencripta el chofer y el cliente antes de devolverlo
        /// </summary>
        /// <param name="idViaje"></param>
        /// <returns>Un viaje solicitado</returns>
        public async Task<Viaje> getViaje(string idViaje)
        {
            try
            {
                Viaje viaje = await DBRepositoryMongo<Viaje>.GetItemAsync(idViaje, "Viajes");
                //viaje.Vehiculo.Chofer = viaje.Vehiculo.Chofer.Desencriptar(viaje.Vehiculo.Chofer);
                //viaje.Cliente = viaje.Cliente.Desencriptar(viaje.Cliente);
                viaje = Utilidades.desencriptarViaje(viaje);
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
        /// se devuelve lista de viajes que se encuentrar pendientes o en curso que hay en la bd
        /// tanto viajes online como viajes directos(ingresados desde el backend)
        /// se desencriptan los cliente y los choferes antes de devolverlos
        /// </summary>
        /// <returns>lista de viajes</returns>
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
        /// se devuelve el viaje pendiente sin confirmar con el id pasado como parametro
        /// </summary>
        /// <param name="idViaje"></param>
        /// <returns>Viaje</returns>
        public async Task<Viaje> getViajePendiente(string idViaje)
        {
            try
            {
                Viaje viaje = await DBRepositoryMongo<Viaje>.GetItemAsync(idViaje, "ViajesPendientes");
                if (viaje != null)
                {
                    // viaje.Vehiculo.Chofer = viaje.Vehiculo.Chofer.Desencriptar(viaje.Vehiculo.Chofer);
                    viaje = Utilidades.desencriptarViaje(viaje);
                }
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
        ///lista de viajes online ingresados al sistema
        /// </summary>
        /// <returns>Lista de viajes</returns>
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
        /// lista de viajes ingresados desde el backend
        /// </summary>
        /// <returns>Lista de viajes</returns>
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
        /// devuelve la lista de viajes activos del chofer
        /// Incluye viajes online y directos que no se encuentren finalizados
        /// </summary>
        /// <param name="idChofer"></param>
        /// <returns>Lista de viajes</returns>
        public async Task<List<Viaje>> getViajesActivosChofer(string idChofer)
        {
            var viajesOnline = await DBRepositoryMongo<Viaje>.GetItemsAsync("Viajes");
            List<Viaje> auxOnline = viajesOnline.Where(v => v.Vehiculo.Chofer.Id.ToString().Equals(idChofer)).Where(v => !v.Estado.Equals(EstadoViaje.Finalizado)).Where(v => !v.Estado.Equals(EstadoViaje.Cancelado)).ToList();
            var viajesDirectos = await DBRepositoryMongo<Viaje>.GetItemsAsync("ViajesDirectos");
            List<Viaje> auxDirecto = viajesDirectos.Where(v => v.Vehiculo.Chofer.Id.ToString().Equals(idChofer)).Where(v => !v.Estado.Equals(EstadoViaje.Finalizado)).Where(v => !v.Estado.Equals(EstadoViaje.Cancelado)).ToList();
            List<Viaje> salida = new List<Viaje>();
            foreach (Viaje v in auxOnline)
            {
                v.Vehiculo.Chofer = v.Vehiculo.Chofer.Desencriptar(v.Vehiculo.Chofer);
                v.Cliente = v.Cliente.Desencriptar(v.Cliente);
                salida.Add(v);
            }
            foreach (Viaje v in auxDirecto)
            {
                v.Vehiculo.Chofer = v.Vehiculo.Chofer.Desencriptar(v.Vehiculo.Chofer);
                v.Cliente = v.Cliente.Desencriptar(v.Cliente);
                salida.Add(v);
            }
            return salida;

        }

        /// <summary>
        /// Todos los viajes de un chofer que tiene el id pasado como parametro
        /// </summary>
        /// <param name="idChofer"></param>
        /// <returns>Lista de viajes</returns>
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

        /// <summary>
        /// liquidacion de un chofer con los viajes cargados
        /// </summary>
        /// <param name="idChofer"></param>
        /// <param name="idUsuario"></param>
        /// <returns>liquidacion</returns>
        public async Task<LiquidacionChofer> getLiquidacionChofer(string idChofer, string idUsuario)
        {
            try
            {
                LiquidacionChofer liquidacion = new LiquidacionChofer();
                Chofer chofer = await ControladoraUsuarios.getInstance(_settings).getChofer(idChofer);
                if (chofer != null)
                {
                    liquidacion.Chofer = chofer;
                    List<Viaje> paraLiquidar = await getViajesParaLiquidarChofer(chofer);
                    liquidacion.Viajes = paraLiquidar;
                    liquidacion.FechaLiquidacion = DateTime.Now;
                    var administrativo = await ControladoraUsuarios.getInstance(_settings).getAdministrativo(idUsuario);
                    liquidacion.Administrativo = administrativo.User;
                }

                return liquidacion;
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
        /// lista de viajes para liquidar un chofer.
        /// se toman en cuenta solo los finalizados o cancelados
        /// </summary>
        /// <param name="chofer"></param>
        /// <returns></returns>
        public async Task<List<Viaje>> getViajesParaLiquidarChofer(Chofer chofer)
        {
            try
            {
                List<Viaje> salida = new List<Viaje>();
                var viajes = await getViajes();
                salida = viajes.Where(v => !v.Liquidado).Where(v => v.Estado.Equals(EstadoViaje.Finalizado)).Where(v => v.Estado == EstadoViaje.Cancelado).Where(v => v.Vehiculo.Chofer.Id.Equals(chofer.Id)).ToList();
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
        /// lista de liquidaciones realizadas
        /// </summary>
        /// <returns>lista liquidacion chofer</returns>
        public async Task<List<LiquidacionChofer>> getLiquidacionesRealizadas()
        {
            try
            {
                var aux = await DBRepositoryMongo<LiquidacionChofer>.GetItemsAsync("Liquidaciones");
                List<LiquidacionChofer> liquidacionesRealizadas = aux.Where(l => !l.Pendiente).ToList();
                return liquidacionesRealizadas;
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

        /// <summary>
        /// guarda el viaje en la coleccion deseada y antes encripta los datos del chofer y el cliente
        /// </summary>
        /// <param name="viaje"></param>
        /// <param name="coleccion"></param>
        /// <returns></returns>
        public async Task guardarViaje(Viaje viaje, string coleccion)
        {
            try
            {
                viaje = Utilidades.encriptarViaje(viaje);
                await DBRepositoryMongo<Viaje>.Create(viaje, coleccion);
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
        /// actualiza los datos del viaje en la coleccion deseada y antes encripta los datos del cliente y el chofer.
        /// </summary>
        /// <param name="viaje"></param>
        /// <param name="coleccion"></param>
        /// <returns></returns>
        public async Task actualizarViaje(Viaje viaje, string coleccion)
        {
            try
            {
                viaje = Utilidades.encriptarViaje(viaje);
                await DBRepositoryMongo<Viaje>.UpdateAsync(viaje.Id, viaje, coleccion);
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
        /// recibe un viaje inicializado, el tipo de vehiculo deseado(puede ser un flete comun o mudanza),
        /// le termina de cargar datos, le asigna el mejor vehiculo, lo guarda o lo actualiza como pendiente de confirmacion 
        /// segun sea el caso
        /// </summary>
        /// <param name="viaje"></param>
        /// <param name="tipoVehiculo"></param>
        /// <returns>viaje</returns>
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
                    unidadesTraslado += ControladoraVehiculos.getInstance(_settings).calcularUnidades((double)i.Alto, (double)i.Ancho, (double)i.Profundidad);
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
                    //Vehiculo vehiculoDisponible = null;
                    if (viaje.Vehiculo == null || viaje.Vehiculo.Id.ToString().Equals("000000000000000000000000"))
                    {
                        if (tipoVehiculo == TipoVehiculo.CamionMudanza)
                        {
                            viaje.Vehiculo = await ControladoraVehiculos.getInstance(_settings).mejorVehiculoMudanza(latitudOrigen, longitudOrigen);
                        }
                        else
                        {
                            viaje.Vehiculo = await ControladoraVehiculos.getInstance(_settings).mejorVehiculoFlete(latitudOrigen, longitudOrigen, unidadesTraslado, pesoTotal);
                        }
                    }

                    if (viaje.Vehiculo != null)
                    {
                        if (!viaje.Vehiculo.Id.ToString().Equals("000000000000000000000000"))
                        {
                            //viaje.Vehiculo = vehiculoDisponible;
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
                            viaje.CantidadKm = (double)await ControladoraVehiculos.getInstance(_settings).cantidadKmAproximado(viaje);
                            viaje.CostoEstimadoFinal = calcularPrecio(viaje.DuracionEstimadaTotal, viaje.Vehiculo.Tarifa, viaje.Compartido);
                            Viaje aux = await DBRepositoryMongo<Viaje>.GetItemAsync(viaje.Id.ToString(), "ViajesPendientes");
                            if (aux != null)
                            {
                                await actualizarViaje(viaje, "ViajesPendientes");
                                //await DBRepositoryMongo<Viaje>.UpdateAsync(viaje.Id, viaje, "ViajesPendientes");
                            }
                            else
                            {
                                //viaje.Cliente = viaje.Cliente.Encriptar(viaje.Cliente);
                                //viaje.Vehiculo.Chofer = viaje.Vehiculo.Chofer.Encriptar(viaje.Vehiculo.Chofer);
                                //await DBRepositoryMongo<Viaje>.Create(viaje, "ViajesPendientes");
                                await guardarViaje(viaje, "ViajesPendientes");
                            }
                            ControladoraVehiculos.getInstance(_settings).actualizarVehiculo(viaje.Vehiculo); //refresco el estado del vehiculo en base y en memoria
                            return viaje;
                        }
                        else
                        {
                            return viaje;
                        }
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

        /// <summary>
        /// guarda un nuevo presupuesto
        /// </summary>
        /// <param name="nuevo"></param>
        /// <returns></returns>
        public async Task presupuestoNuevo(Presupuesto nuevo)
        {
            try
            {
                await DBRepositoryMongo<Presupuesto>.Create(nuevo, "Presupuestos");
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
        /// se confirma el viaje con el id pasado como parametro
        /// se quita de viajes pendientes de confirmacion y se pasa a viajes pendientes de inicar
        /// a la espera de que el chofer seleccionado de comienzo a la actividad
        /// </summary>
        /// <param name="idViaje"></param>
        /// <returns>viaje</returns>
        public async Task<Viaje> confirmarViaje(string idViaje)
        {
            try
            {
                //Viaje viaje = await DBRepositoryMongo<Viaje>.GetItemAsync(idViaje, "ViajesPendientes");
                Viaje viaje = await getViajePendiente(idViaje);
                viaje.ConfirmacionCliente = true;
                TimeZoneInfo timeZone = TimeZoneInfo.Local;
                viaje.FechaConfirmacionCliente = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone);
                //await DBRepositoryMongo<Viaje>.Create(viaje, "Viajes");
                await guardarViaje(viaje, "Viajes");
                await DBRepositoryMongo<Viaje>.DeleteAsync(idViaje, "ViajesPendientes");
                //viaje.Cliente.Desencriptar(viaje.Cliente);
                //viaje.Vehiculo.Chofer = viaje.Vehiculo.Chofer.Desencriptar(viaje.Vehiculo.Chofer);
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
        /// si el viaje todavia no habia sido confirmado se cancela y se elimina de los viajes pendientes de confirmacion
        /// si ya habia sido confirmado se calcula el costo de cancelacion del viaje y se devuelve para la decision del cliente
        /// si el viaje ya habia sido iniciado por el chofer se devuelve -1 y no se puede cancelar
        /// </summary>
        /// <param name="idViaje"></param>
        /// <returns>double</returns>
        public async Task<double> cancelarViaje(string idViaje)
        {
            try
            {

                double costo = 0;
                Viaje viaje = await DBRepositoryMongo<Viaje>.GetItemAsync(idViaje, "ViajesPendientes");
                if (viaje != null)
                {
                    foreach (Vehiculo v in ControladoraVehiculos.getInstance(_settings).Vehiculos)
                    {
                        if (v.Id.ToString().Equals(viaje.Vehiculo.Id.ToString()))
                        {
                            v.Disponible = true;
                            if (viaje.Items.Count > 0)
                            {
                                foreach (Item i in viaje.Items)
                                {
                                    Item eliminar = null;
                                    foreach (Item item in v.Items)
                                    {
                                        if (i.Id.ToString().Equals(item.Id.ToString()))
                                        {
                                            eliminar = item;
                                        }
                                    }
                                    v.Items.Remove(eliminar);
                                }
                            }
                        }
                    }
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
                            costo = confirmado.Vehiculo.Tarifa * 0.25;
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

        /// <summary>
        /// se confirmo la cancelacion, entonces se busca el viaje con el id seleccionado y se le asigna el costo final pasado como parametro
        /// ademas se le asigna la hora de finalizacion, y se lo marca como cancelado.
        /// ademas se liberan los items del vehiculo participante
        /// </summary>
        /// <param name="idViaje"></param>
        /// <param name="costoFinal"></param>
        /// <returns></returns>
        public async Task confirmarCancelacion(string idViaje, double costoFinal)
        {
            try
            {
                if (!string.IsNullOrEmpty(idViaje))
                {
                    Viaje cancelado = await getViaje(idViaje);
                    cancelado.Estado = EstadoViaje.Cancelado;
                    cancelado.CostoFinal = costoFinal;
                    TimeZoneInfo timeZone = TimeZoneInfo.Local;
                    cancelado.HoraFin = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone).TimeOfDay;
                    string idVehiculo = cancelado.Vehiculo.Id.ToString();
                    foreach (Vehiculo v in ControladoraVehiculos.getInstance(_settings).Vehiculos)
                    {
                        if (v.Id.ToString().Equals(idVehiculo))
                        {
                            v.Disponible = true;
                            if (cancelado.Items.Count > 0)
                            {
                                foreach (Item i in cancelado.Items)
                                {
                                    Item eliminar = null;
                                    foreach (Item item in v.Items)
                                    {
                                        if (i.Id.ToString().Equals(item.Id.ToString()))
                                        {
                                            eliminar = item;
                                        }
                                    }
                                    v.Items.Remove(eliminar);
                                }
                            }
                        }
                    }
                    cancelado.Cliente = cancelado.Cliente.Encriptar(cancelado.Cliente);
                    cancelado.Vehiculo.Chofer = cancelado.Vehiculo.Chofer.Encriptar(cancelado.Vehiculo.Chofer);
                    await DBRepositoryMongo<Viaje>.UpdateAsync(cancelado.Id, cancelado, "Viajes");
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
        /// se cambia el estado del viaje con el id pasado como paramtro a en curso
        /// </summary>
        /// <param name="idViaje"></param>
        /// <returns>viaje</returns>
        public async Task<Viaje> levanteViaje(string idViaje)
        {
            try
            {
                Viaje actualizar = await getViaje(idViaje);
                actualizar.Estado = EstadoViaje.EnCurso;
                //actualizar.Cliente = actualizar.Cliente.Encriptar(actualizar.Cliente);
                //actualizar.Vehiculo.Chofer = actualizar.Vehiculo.Chofer.Encriptar(actualizar.Vehiculo.Chofer);
                TimeZoneInfo timeZone = TimeZoneInfo.Local;
                actualizar.HoraInicio = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone).TimeOfDay;
                //await DBRepositoryMongo<Viaje>.UpdateAsync(actualizar.Id, actualizar, "Viajes");
                await actualizarViaje(actualizar, "Viajes");
                return actualizar;
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
        /// se busca el viaje pendiente de confirmacion de un cliente con el id pasado como parametro
        /// </summary>
        /// <param name="cliente"></param>
        /// <returns>viaje</returns>
        public async Task<Viaje> viajePendienteCliente(string cliente)
        {
            try
            {

                var viajes = await DBRepositoryMongo<Viaje>.GetItemsAsync("ViajesPendientes");
                Viaje viajePendiente = viajes.FirstOrDefault(v => v.Cliente.Id.ToString().Equals(cliente));
                if (viajePendiente != null && viajePendiente.Cliente != null)
                {
                    viajePendiente = Utilidades.desencriptarViaje(viajePendiente);
                    //viajePendiente.Cliente.Desencriptar(viajePendiente.Cliente);
                    //if(va)
                    //if (!viajePendiente.Vehiculo.Id.Equals("000000000000000000000000"))
                    //{
                    //    viajePendiente.Vehiculo.Chofer.Desencriptar(viajePendiente.Vehiculo.Chofer);
                    //}
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

        /// <summary>
        /// se le agrega el item pasado como parametro al viaje pendiente de confirmacion pasado como paramtro
        /// </summary>
        /// <param name="viaje"></param>
        /// <param name="item"></param>
        /// <returns></returns>
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
                            nuevo.Origen = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(viaje.DireccionOrigen);
                        }

                    }
                    else
                    {
                        nuevo.DireccionOrigen = item.DireccionOrigen;
                        nuevo.Origen = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(item.DireccionOrigen);
                    }
                    if (nuevo.Items.Count > 0)
                    {
                        foreach (Item i in nuevo.Items)
                        {
                            i.Origen = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(i.DireccionOrigen);
                            i.Destino = await ControladoraVehiculos.getInstance(_settings).convertirDireccionEnCoordenadas(i.DireccionDestino);
                        }
                    }
                    //nuevo.Cliente = nuevo.Cliente.Encriptar(nuevo.Cliente);
                    //await DBRepositoryMongo<Viaje>.Create(nuevo, "ViajesPendientes");
                    await guardarViaje(nuevo, "ViajesPendientes");
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
                    //aux.Cliente.Encriptar(aux.Cliente);

                    //await DBRepositoryMongo<Viaje>.UpdateAsync(aux.Id, aux, "ViajesPendientes");
                    await actualizarViaje(aux, "ViajesPendientes");
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
        /// se recorren todos los items del viaje pasado como parametro y si no tiene direccion origen o destino se les asiga
        /// la direccion origen o destino (dependiendo del caso) del viaje
        /// ademas se cargarn las posiciones satelitales de los items
        /// </summary>
        /// <param name="viaje"></param>
        /// <returns>viaje</returns>
        public async Task<Viaje> corroborarDireccionesItems(Viaje viaje)
        {
            try
            {
                foreach (Item i in viaje.Items)
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
        /// se ingresa un nuevo viaje (directo) desde el backend
        /// </summary>
        /// <param name="idVehiculo"></param>
        /// <param name="idCliente"></param>
        /// <param name="direccion"></param>
        /// <param name="fecha"></param>
        /// <param name="horaInicio"></param>
        /// <param name="comentarios"></param>
        /// <returns></returns>
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

        /// <summary>
        /// se modifica un viaje ingresado desde el backend
        /// no deja modificarlo si esta en curso
        /// </summary>
        /// <param name="id"></param>
        /// <param name="viaje"></param>
        /// <returns></returns>
        public async Task modificarViaje(string id, Viaje viaje)
        {
            try
            {
                if (id != null && viaje != null)
                {
                    viaje.Id = new ObjectId(id);
                    Viaje v = await DBRepositoryMongo<Viaje>.GetItemAsync(id, "ViajesDirectos");
                    if (v.Estado.Equals(EstadoViaje.Pendiente))
                    {
                        await DBRepositoryMongo<Viaje>.UpdateAsync(viaje.Id, viaje, "ViajesDirectos");
                    }
                    else
                    {
                        throw new MensajeException("No se puede modificar el viaje, ya se encuentra en curso");
                    }
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

        /// <summary>
        /// elimina un viaje ingresado desde el backend
        /// no permite eliminar un viaje en curso
        /// </summary>
        /// <param name="id"></param>
        /// <param name="viaje"></param>
        /// <returns></returns>
        public async Task eliminarViaje(string id, Viaje viaje)
        {
            try
            {
                if (id != null && viaje != null)
                {
                    Viaje v = await DBRepositoryMongo<Viaje>.GetItemAsync(id, "ViajesDiretos");
                    if (v.Estado.Equals(EstadoViaje.Pendiente))
                    {
                        await DBRepositoryMongo<Viaje>.DeleteAsync(id, "ViajesDirectos");
                    }
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

        /// <summary>
        /// se da por finalizado el viaje con el id pasado como parametro
        /// se le actualizan los datos de horas, costos, etc.
        /// </summary>
        /// <param name="idViaje"></param>
        /// <returns>viaje</returns>
        public async Task<Viaje> finalizarViaje(string idViaje)
        {
            Viaje salida = await getViaje(idViaje);
            if (salida != null && salida.Estado != EstadoViaje.Finalizado && salida.Estado != EstadoViaje.Cancelado)
            {
                salida.Estado = EstadoViaje.Finalizado;
                TimeZoneInfo timeZone = TimeZoneInfo.Local;
                salida.HoraFin = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone).TimeOfDay;
                TimeSpan total = salida.HoraFin.Subtract(salida.HoraInicio);
                double costo = calcularPrecio(total, salida.Vehiculo.Tarifa, salida.Compartido);
                salida.CostoFinal = costo;
                salida.Estado = EstadoViaje.Finalizado;
                await actualizarViaje(salida, "Viajes");
                salida.Vehiculo.Disponible = true;
                if (salida.Items.Count > 0) // me fijo si el viaje tiene items. si tiene items es porque se esta cancelando desde el backend
                {
                    salida.Vehiculo.Items = new List<Item>();
                }
                ControladoraVehiculos.getInstance(_settings).actualizarVehiculo(salida.Vehiculo);
                return salida;
            }
            return salida;
        }

        /// <summary>
        /// se calcula el precio de un viaje segun el tiempo de viaje, la tarifa del vehiculo y si es compartido o no
        /// para esto se tomaron en cuenta reglas de negocio
        /// </summary>
        /// <param name="tiempo"></param>
        /// <param name="tarifa"></param>
        /// <param name="compartido"></param>
        /// <returns>double</returns>
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

        /// <summary>
        /// se le agrega la lista de items pasada como parametro a vehiculo pasado como parametro
        /// </summary>
        /// <param name="paraLlevar"></param>
        /// <param name="vehiculo"></param>
        /// <returns>vehiculo</returns>
        public Vehiculo agreagrItemsaVehiculo(List<Item> paraLlevar, Vehiculo vehiculo)
        {
            foreach (Item i in paraLlevar)
            {
                vehiculo.Items.Add(i);
            }
            return vehiculo;
        }

        /// <summary>
        /// obtiene la ultima tarifa ingresada en la bd
        /// </summary>
        /// <returns>tarifa</returns>
        public async Task<Tarifa> obtenerUltimaTarifa()
        {
            try
            {
                var tarifas = await DBRepositoryMongo<Tarifa>.GetItemsAsync("Tarifas");
                Tarifa ultima = tarifas.OrderByDescending(t => t.FechaModificacion).FirstOrDefault();
                return ultima;
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
        /// guarda en la bd la tarifa y le asigna el usuario que la modifico
        /// </summary>
        /// <param name="nueva"></param>
        /// <param name="idUsuario"></param>
        /// <returns>tarifa</returns>
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
        /// termina de asignar los valores a la liquidacion pasada como parametro.
        /// la guarda como pendiente en la bd
        /// </summary>
        /// <param name="liquidacion"></param>
        /// <returns>liquidacion chofer</returns>
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
        /// se confirma la liquidacion con id pasado como parametro y se le cambia el estado a realizada
        /// ademas se marcan todos los viajes que tenia como liquidados
        /// </summary>
        /// <param name="idLiquidacionChofer"></param>
        /// <returns></returns>
        public async Task confirmarLiquidacion(string idLiquidacionChofer)
        {
            try
            {
                LiquidacionChofer liquidacion = await DBRepositoryMongo<LiquidacionChofer>.GetItemAsync(idLiquidacionChofer, "Liquidaciones");
                if (liquidacion != null)
                {
                    List<Viaje> directos = await getViajesDirectos();
                    List<Viaje> online = await getViajesOnline();
                    foreach (Viaje v in liquidacion.Viajes)
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
        /// se cancela liquidacion con id pasado como parametro 
        /// se saca de la base como liquidacion pendiente
        /// </summary>
        /// <param name="idLiquidacion"></param>
        /// <returns></returns>
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

        /// <summary>
        /// se devuelve una lista con los presupuestos que estan pendientes de realizacion
        /// </summary>
        /// <returns> listapresupuesto</returns>
        public async Task<List<Presupuesto>> presupuestosPendientes()
        {
            try
            {
                var aux = await DBRepositoryMongo<Presupuesto>.GetItemsAsync("Presupuestos");
                List<Presupuesto> pendientes = aux.Where(p => p.Realizado == false).ToList();
                return pendientes;
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
        /// se confirma un presupuesto con id pasado como parametro como realizado y
        /// se modifica en la bd
        /// </summary>
        /// <param name="idPresupuesto"></param>
        /// <returns></returns>
        public async Task confirmarPresupuesto(string idPresupuesto)
        {
            try
            {
                Presupuesto presupuesto = await DBRepositoryMongo<Presupuesto>.GetItemAsync(idPresupuesto, "Presupuestos");
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

        /// <summary>
        /// genera estadisticas para el vehiculo con el id pasado como parametro en el año seleccionado y en el mes seleccionado
        /// </summary>
        /// <param name="año"></param>
        /// <param name="mes"></param>
        /// <param name="idVehiculo"></param>
        /// <returns>estadistica vehiculo</returns>
        public async Task<EstadisticaVehiculo> estadisticaVehiculo(string año, string mes, string idVehiculo)
        {
            try
            {
                var auxViajesOnline = await DBRepositoryMongo<Viaje>.GetItemsAsync("Viajes");
                var auxViajesDirectos = await DBRepositoryMongo<Viaje>.GetItemsAsync("ViajesDirectos");
                List<Viaje> viajesOnline = auxViajesOnline.Where(v => v.Estado == EstadoViaje.Finalizado).Where(v => v.Estado == EstadoViaje.Cancelado).Where(v => v.Vehiculo.Id.ToString().Equals(idVehiculo)).ToList();
                List<Viaje> viajesDirectos = auxViajesDirectos.Where(v => v.Estado == EstadoViaje.Finalizado).Where(v => v.Estado == EstadoViaje.Cancelado).Where(v => v.Vehiculo.Id.ToString().Equals(idVehiculo)).ToList();
                int cantidadViajesDirectos = 0;
                int cantidadViajesOnline = 0;
                double kmRecorridos = 0;
                double totalRecaudado = 0;
                double cantidadCombustible = 0;


                foreach (Viaje v in viajesOnline)
                {
                    if (!string.IsNullOrEmpty(mes))
                    {
                        if (v.Fecha.Year == int.Parse(año))
                        {
                            if (v.Fecha.Month == int.Parse(mes))
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
                        if (v.Fecha.Year == int.Parse(año))
                        {
                            cantidadViajesOnline += 1;
                            kmRecorridos += v.CantidadKm;
                            totalRecaudado += v.CostoFinal;
                            cantidadCombustible += (v.CantidadKm / v.Vehiculo.ConsumoKml);
                        }
                    }

                }
                foreach (Viaje v in viajesDirectos)
                {
                    if (!string.IsNullOrEmpty(mes))
                    {
                        if (v.Fecha.Year == int.Parse(año))
                        {
                            if (v.Fecha.Month == int.Parse(mes))
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
                        if (v.Fecha.Year == int.Parse(mes))
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
    }
}
