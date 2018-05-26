using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class TarjetaDeCredito
    {
        [JsonProperty(PropertyName = "numero")]
        public long Numero { get; set; }
        [JsonProperty(PropertyName = "fVencimiento")]
        public DateTime fVencimiento { get; set; }
    }
}
