using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
//using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Viaje
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public Vehiculo Vehiculo { get; set; }
        public Cliente Cliente { get; set; }
        public List<Item> Items { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public double Calificacion { get; set; }
        public string Comentarios { get; set; }
        public EstadoViaje Estado { get; set; }
        public double CostoFinal { get; set; }
        public double CostoEstimadoFinal { get; set; }
        public TimeSpan DuracionEstimadaHastaCliente { get; set; }
        public TimeSpan DuracionEstimadaTotal { get; set; }
        [Required(ErrorMessage ="El destino no puede ser vacio")]
        public string DireccionDestino { get; set; }
        public string DireccionOrigen { get; set; }
        public bool Compartido { get; set; }
        public PosicionSatelital Origen { get; set; }
        public PosicionSatelital Destino { get; set; }
        public bool Liquidado { get; set; }
        public bool ConfirmacionCliente { get; set; }
        public DateTime FechaConfirmacionCliente { get; set; }
        public double CantidadKm { get; set; }


        #region Constructores
        public Viaje() { Items = new List<Item>(); }
        #endregion
    }
}
