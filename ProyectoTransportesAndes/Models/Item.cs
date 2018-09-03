﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    
    public class Item
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public double? Alto { get; set; }
        public double? Ancho { get; set; }
        public double? Profundidad { get; set; }
        public double? Peso { get; set; }
        public string Descripcion { get; set; }
        public string Imagen { get; set; }
        public TipoItem Tipo { get; set; }
        public string DireccionOrigen { get; set; }
        public string DireccionDestino { get; set; }
        public PosicionSatelital Origen { get; set; }
        public PosicionSatelital Destino { get; set; }
    }
}
