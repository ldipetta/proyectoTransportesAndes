﻿using Microsoft.Extensions.Options;
using ProyectoTransportesAndes.Configuracion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProyectoTransportesAndes.Persistencia;
using System.Collections;

namespace ProyectoTransportesAndes.Models
{
    public class ControladoraViajes
    {
        #region "Atributos"
        private static ControladoraViajes _instancia;
        private IOptions<AppSettingsMongo> _settings;
        private Hashtable _ubicacionesClientes;
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
            _ubicacionesClientes = new Hashtable();
            DBRepositoryMongo<Viaje>.Iniciar(_settings);
            DBRepositoryMongo<Cliente>.Iniciar(_settings);
        }
        #endregion

        #region "Metodos"
        public async Task<Viaje> getViaje(string idViaje)
        {
            Viaje viaje = await DBRepositoryMongo<Viaje>.GetItemAsync(idViaje, "Viajes");
            return viaje;
        }
        public async Task<Viaje> solicitarViaje(string idViaje,string idCliente)
        {
            Viaje viaje = await DBRepositoryMongo<Viaje>.GetItemAsync(idViaje, "ViajesPendientes");
            viaje.Fecha = DateTime.Today;
            viaje.Cliente = await DBRepositoryMongo<Cliente>.GetItemAsync(idCliente,"Clientes");
            viaje.Estado = EstadoViaje.EnCurso;
            double unidadesTraslado = 0;
            double pesoTotal = 0;
            foreach(Item i in viaje.Items)
            {
                unidadesTraslado += ControladoraVehiculos.getInstance(_settings).calcularUnidades(i.Alto, i.Ancho, i.Profundidad);
                pesoTotal += i.Peso;
            }
            PosicionSatelital posicion = obtenerUbicacionCliente(idCliente);
            if (posicion != null)
            {
                string latitudOrigen = posicion.Latitud;
                string longitudOrigen = posicion.Longitud;
                Vehiculo vehiculoDisponible = await ControladoraVehiculos.getInstance(_settings).mejorVehiculoPrueba(latitudOrigen, longitudOrigen, unidadesTraslado, pesoTotal);
                if (vehiculoDisponible != null)
                {
                    viaje.Vehiculo = vehiculoDisponible;
                    viaje.Vehiculo.CapacidadCargaKg -= pesoTotal;
                    viaje.Vehiculo.Unidades -= unidadesTraslado;
                    viaje.HoraInicio = DateTime.UtcNow.TimeOfDay;
                    viaje.DuracionEstimada = await ControladoraVehiculos.getInstance(_settings).tiempoDemora(latitudOrigen, longitudOrigen, viaje.Vehiculo.PosicionSatelital.Latitud, viaje.Vehiculo.PosicionSatelital.Longitud);
                    await DBRepositoryMongo<Viaje>.Create(viaje, "Viajes");
                    await DBRepositoryMongo<Viaje>.DeleteAsync(viaje.Id, "ViajesPendientes");
                    return viaje;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
           
        }
        public async Task<Viaje>viajePendienteCliente(string cliente)
        {
            var viajes = await DBRepositoryMongo<Viaje>.GetItemsAsync("ViajesPendientes");
            Viaje viajePendiente = viajes.FirstOrDefault(v => v.Cliente.Id.ToString().Equals(cliente));
            return viajePendiente;
        }
        public async Task agregarItem(string idCliente, Item item)
        {
            Viaje viaje = await viajePendienteCliente(idCliente);
            var cliente = await DBRepositoryMongo<Cliente>.GetItemAsync(idCliente, "Clientes");
            if (viaje == null)
            {
                Viaje nuevo = new Viaje();
                nuevo.Estado = EstadoViaje.Pendiente;
                nuevo.Items = new List<Item>();
                nuevo.Items.Add(item);
                nuevo.Cliente = cliente;
                await DBRepositoryMongo<Viaje>.Create(nuevo, "ViajesPendientes");
            }
            else
            {
                viaje.Items.Add(item);
                await DBRepositoryMongo<Viaje>.UpdateAsync(viaje.Id, viaje, "ViajesPendientes");
            }
        }
        public async Task<Item> itemParaEditar(string idCliente, string idItem)
        {
            Viaje viaje = await viajePendienteCliente(idCliente);
            Item item = null;
            foreach(Item i in viaje.Items)
            {
                if (i.Id.ToString().Equals(idItem))
                {
                    item = i;
                }
            }
            return item;
        }
        public async Task editarItem(string idCliente, Item item)
        {
            Viaje viaje = await viajePendienteCliente(idCliente);
            foreach(Item i in viaje.Items)
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
        public async Task finalizarViaje(Viaje viaje)
        {
            if (viaje != null)
            {
                viaje.Estado = EstadoViaje.Finalizado;
                viaje.HoraFin = DateTime.UtcNow.TimeOfDay;
                TimeSpan total = viaje.HoraFin.Subtract(viaje.HoraInicio);
                double costo = calcularPrecio(total,viaje.Vehiculo.Tarifa);
                viaje.CostoFinal = costo;
                viaje.Estado = EstadoViaje.Finalizado;
                await DBRepositoryMongo<Viaje>.UpdateAsync(viaje.Id,viaje, "Viajes");
            }
        }
        public async Task<List<Viaje>> viajesCliente(string idCliente)
        {
            var items = await DBRepositoryMongo<Viaje>.GetItemsAsync("Viajes");
            items.Where(v => v.Cliente.Id.Equals(idCliente)).Where(v => v.Estado == EstadoViaje.Finalizado);
            return items.ToList();
        }
        public PosicionSatelital guardarUbicacionCliente(string idCliente, string latitud, string longitud)
        {
            PosicionSatelital posicion = new PosicionSatelital(idCliente, latitud, longitud);
            if (_ubicacionesClientes.Contains(idCliente))
            {
                _ubicacionesClientes[idCliente] = posicion;
            }
            else
            {
                _ubicacionesClientes.Add(posicion.Id, posicion);
            }
            return posicion;
        }
        public PosicionSatelital obtenerUbicacionCliente(string idCliente)
        {
            return (PosicionSatelital)_ubicacionesClientes[idCliente];
        }
        public double calcularPrecio(TimeSpan tiempo, int tarifa)
        {
            double precio = 0;
            if (tiempo.TotalMinutes>60)
            {
                precio = (tiempo.TotalMinutes / 60) * tarifa;
            }
            else
            {
                precio = tarifa;
            }
            return precio;
        }
        #endregion
    }
}
