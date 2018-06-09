using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ProyectoTransportesAndes.Models
{

    public class Usuario
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string User { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Documento { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string Telefono { get; set; }
        
        public string Direccion { get; set; }
        [DataType(DataType.Date)]
        public string FNacimiento { get; set; }
        public string Tipo { get; set; }
        

    
        public static string validarToken(string token)
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
        public static string BuildToken(Usuario user,IConfiguration configuration)
        {
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.UniqueName,user.User),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Actort,user.Tipo)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Llave"]));
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

    }
}
