using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ProyectoTransportesAndes.Models
{
    public class TarjetaDeCredito
    {
        
        [Display(Name ="Numero tarjeta")]
        [DataType(DataType.CreditCard)]
        public string Numero { get; set; }
        [Display(Name ="Fecha de vencimiento")]
        [DataType(DataType.Date)]
        public string fVencimiento { get; set; }
    }
}
