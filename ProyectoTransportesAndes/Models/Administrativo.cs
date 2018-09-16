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
        public bool Administrador { get; set; }
        #endregion

        #region Metodos
        /// <summary>
        /// recibe un usuario desencriptado y lo devuelve encriptado
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns>usuario encriptado</returns>
        public Administrativo Encriptar(Administrativo usuario)
        {
            usuario.Apellido = Seguridad.Encriptar(usuario.Apellido);
            usuario.Direccion = Seguridad.Encriptar(usuario.Direccion);
            usuario.Documento = Seguridad.Encriptar(usuario.Documento);
            usuario.Email = Seguridad.Encriptar(usuario.Email);
            usuario.FNacimiento = Seguridad.Encriptar(usuario.FNacimiento);
            usuario.Nombre = Seguridad.Encriptar(usuario.Nombre);
            usuario.Password = Seguridad.Encriptar(usuario.Password);
            usuario.Telefono = Seguridad.Encriptar(usuario.Telefono);
            usuario.Tipo = Seguridad.Encriptar(usuario.Tipo);
            usuario.User = Seguridad.Encriptar(usuario.User);
            usuario.Ubicacion.Latitud = Seguridad.Encriptar(usuario.Ubicacion.Latitud);
            usuario.Ubicacion.Longitud = Seguridad.Encriptar(usuario.Ubicacion.Longitud);
            return usuario;
        }
        /// <summary>
        /// recibe un usuario encriptado y lo devuelve desencriptado
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns>usuario desencriptado</returns>
        public Administrativo Desencriptar(Administrativo usuario)
        {
            usuario.Apellido = Seguridad.Desencriptar(usuario.Apellido);
            usuario.Direccion = Seguridad.Desencriptar(usuario.Direccion);
            usuario.Documento = Seguridad.Desencriptar(usuario.Documento);
            usuario.Email = Seguridad.Desencriptar(usuario.Email);
            usuario.FNacimiento = Seguridad.Desencriptar(usuario.FNacimiento);
            usuario.Nombre = Seguridad.Desencriptar(usuario.Nombre);
            usuario.Password = Seguridad.Desencriptar(usuario.Password);
            usuario.Telefono = Seguridad.Desencriptar(usuario.Telefono);
            usuario.Tipo = Seguridad.Desencriptar(usuario.Tipo);
            usuario.User = Seguridad.Desencriptar(usuario.User);
            usuario.Ubicacion.Latitud = Seguridad.Desencriptar(usuario.Ubicacion.Latitud);
            usuario.Ubicacion.Longitud = Seguridad.Desencriptar(usuario.Ubicacion.Longitud);
            return usuario;
        }
        public Usuario DesencriptarSuperUsuario(Usuario usuario)
        {
            usuario.Password = Seguridad.Desencriptar(usuario.Password);
            usuario.User = Seguridad.Desencriptar(usuario.User);
            return usuario;
        }
        #endregion


    }
}
