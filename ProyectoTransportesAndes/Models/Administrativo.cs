using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ProyectoTransportesAndes.Models
{
    public class Administrativo:Usuario
    {
      

        public CarneDeSalud CarneDeSalud { get; set; }
    }
}
