using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ProyectoTransportesAndes.Models
{
    public class Administrativo:Usuario
    {
        #region Propiedades
        public CarneDeSalud CarneDeSalud { get; set; }
        #endregion

        
    }
}
