using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoTransportesAndes.Models
{
    public class LibretaDeConducir
    {
        
        public string Categoria { get; set; }
        [Display(Name ="Fecha de vencimiento")]
        [DataType(DataType.Date)]
        public string FVencimiento { get; set; }
    }
}
