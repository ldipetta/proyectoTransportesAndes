using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.Exceptions
{
    public class MensajeException:Exception
    {
        #region Constructores
        public MensajeException() { }
        public MensajeException(string mensaje) : base(mensaje) { }
        #endregion
    }
}
