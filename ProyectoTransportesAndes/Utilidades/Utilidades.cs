using MongoDB.Bson;
using ProyectoTransportesAndes.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.Models
{
    public static class Utilidades
    {
        /// <summary>
        /// VALIDA QUE LA DIRECCION SEA DENTRO DEL PAIS URUGUAY Y DENTRO DE MONTEVIDEO
        /// </summary>
        /// <param name="direccion"></param>
        public static void validarDireccion(string direccion)
        {
            try
            {
                if (!string.IsNullOrEmpty(direccion))
                {
                    //string numero = direccion.Split(" ")[1].Split(",")[0];
                    //if (!int.TryParse(numero,out int num))
                    //{
                    //    throw new MensajeException("Debe ingresar el número de puerta en la dirección");
                    //}
                    string departamento = direccion.Split(",")[1];
                    if (!departamento.Equals("Montevideo"))
                    {
                        if (!departamento.Equals(" Montevideo"))
                        {
                            throw new MensajeException("El departamento debe ser Montevideo");
                        }
                    }
                    string pais = direccion.Split(",")[2];
                    if (!pais.Equals("Uruguay") && !pais.Equals(" Uruguay"))
                    {
                        throw new MensajeException("El pais debe ser Montevideo");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //throw new MensajeException("Formato de dirección inválida");
            }

        }
        /// <summary>
        /// DESEREALIZA UN JSON CON EL OBJECTID 
        /// </summary>
        /// <param name="idJson"></param>
        /// <returns>UN OBJETO OBJECTID</returns>
        public static ObjectId deserealizarJsonToObjectId(string idJson)
        {
            var aux = BsonDocument.Parse(idJson);
            var timestamp = aux.GetValue("timestamp").ToDouble();
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime time = unixEpoch.AddSeconds(timestamp).ToLocalTime();
            var machine = aux.GetValue("machine").ToInt32();
            var pid = Convert.ToInt16(aux.GetValue("pid").ToInt32());
            var increment = aux.GetValue("increment").ToInt32();
            var creationTime = aux.GetValue("creationTime");
            ObjectId id = new ObjectId(time, machine, pid, increment);
            return id;
        }
    }
}
