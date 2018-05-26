using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class Cliente:Usuario
    {
        [JsonProperty(PropertyName = "tarjeta")]
        public TarjetaDeCredito Tarjeta { get; set; }

    }
}
