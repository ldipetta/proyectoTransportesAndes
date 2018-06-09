using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ProyectoTransportesAndes.Models
{
    public class Cliente:Usuario
    {
        public TarjetaDeCredito Tarjeta { get; set; }

    }
}
