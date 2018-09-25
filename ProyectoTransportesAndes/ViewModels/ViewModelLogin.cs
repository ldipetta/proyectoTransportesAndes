using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.ViewModels
{
    public class ViewModelLogin
    {
        [Display(Name ="Usuario")]
        [Required(ErrorMessage ="Debe ingresar el usuario")]
        public string Usuario { get; set; }
        [Display(Name ="Contraseña")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage ="Debe ingresar la contraseña")]
        public string Password { get; set; }
    }
}
