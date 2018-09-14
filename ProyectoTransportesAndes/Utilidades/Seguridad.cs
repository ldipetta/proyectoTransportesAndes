using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.Models
{
    public static class Seguridad
    {
        /// <summary>
        /// encripta en base 64 el string pasado como parametro
        /// </summary>
        /// <param name="texto"></param>
        /// <returns>string encriptado</returns>
        public static string Encriptar(string texto)
        {
            try
            {
                string salida = string.Empty;
                byte[] encriptado = System.Text.Encoding.Unicode.GetBytes(texto);
                salida = Convert.ToBase64String(encriptado);
                return salida;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// desencripta en base 64 el string pasado como parametro
        /// </summary>
        /// <param name="texto"></param>
        /// <returns>string desencriptado</returns>
        public static string Desencriptar(string texto)
        {
            try
            {
                string salida = string.Empty;
                byte[] desencriptado = Convert.FromBase64String(texto);
                salida = System.Text.Encoding.Unicode.GetString(desencriptado);
                return salida;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
       
        /// <summary>
        /// valida que el token no este vencido y devuelve el tipo de usuario
        /// </summary>
        /// <param name="token"></param>
        /// <returns>devuelve el tipo de usuario</returns>
        public static string validarToken(string token)
        {
            try
            {
                if (!token.Equals(""))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var tokenS = handler.ReadToken(token) as JwtSecurityToken;
                    if (tokenS.ValidTo > DateTime.UtcNow)
                    {
                        var rol = tokenS.Claims.First(c => c.Type == "actort").Value;
                        return rol;
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        /// <summary>
        /// construye el token en base al usuario pasado como parametro
        /// </summary>
        /// <param name="user"></param>
        /// <returns>devuelve un string con el token encriptado</returns>
        public static string BuildToken(Usuario user)
        {
            try
            {
                var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.UniqueName,user.User),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Actort,user.Tipo)
            };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("gjanvowIINFDk4086206ldvnffnhsdonL"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiration = DateTime.UtcNow.AddHours(1);
                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: "yourDomain.com",
                    audience: "yourDomain.com",
                    claims: claims,
                    expires: expiration,
                    signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        /// <summary>
        /// valida que el token pasado sea del tipo administrativo
        /// </summary>
        /// <param name="token"></param>
        /// <returns>retorna true sii se pudo validar</returns>
        public static bool validarUsuarioAdministrativo(string token)
        {
            try
            {
                bool salida = false;
                if (token != null)
                {
                    var rol = validarToken(token);
                    if (rol == "Administrativo" || rol == "Administrador")
                    {
                        salida = true;
                    }
                }
                return salida;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        /// <summary>
        /// valida que el token pasado sea del tipo cliente
        /// </summary>
        /// <param name="token"></param>
        /// <returns>retorna true sii se pudo validar</returns>
        public static bool validarUsuarioCliente(string token)
        {
            try
            {
                bool salida = false;
                if (token != null)
                {
                    var rol = validarToken(token);
                    if (rol == "Cliente")
                    {
                        salida = true;
                    }
                }
                return salida;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        /// <summary>
        /// valida que el token pasado sea del tipo administrador
        /// </summary>
        /// <param name="token"></param>
        /// <returns>retorna true sii se pudo validar</returns>
        public static bool validarUsuarioAdministrador(string token)
        {
            try
            {
                bool salida = false;
                if (token != null)
                {
                    var rol = validarToken(token);
                    if (rol == "Administrador")
                    {
                        salida = true;
                    }
                }
                return salida;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
