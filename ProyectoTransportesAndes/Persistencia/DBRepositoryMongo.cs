using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProyectoTransportesAndes.Exceptions;

namespace ProyectoTransportesAndes.Persistencia
{
    public static class DBRepositoryMongo<T>
    {
        private static IOptions<AppSettingsMongo> _settings;
        private static IMongoDatabase _database = null;
        private static string[] colecciones = { "Viajes", "Administradores", "Administrativos", "Camiones", "Camionetas", "Choferes", "Clientes", "Peones", "ViajesPendientes","RespaldoVehiculos","Presupuestos"};
        private static MongoClient _client;

        public static void Iniciar(IOptions<AppSettingsMongo> settings)
        {
            try
            {
                _settings = settings;
                var client = new MongoClient(_settings.Value.ConnectionString);
                if (client != null)
                {
                    _client = client;
                    _database = client.GetDatabase(_settings.Value.DataBase);
                    if (_database != null)
                    {
                        CrearColecciones(_database);
                    }
                }
            }
            catch (TimeoutException)
            {
                throw new MensajeException("Se agotó el tiempo de espera, compruebe la conexión");
            }
            catch (Exception)
            {
                throw new MensajeException("Ocurrió un error inesperado, intente nuevamente en unos minutos");
            }

        }
        public static void CrearColecciones(IMongoDatabase dataBase)
        {
            try
            {
                for (int i = 0; i < 8; i++)
                {
                    IMongoCollection<T> _coleccion = dataBase.GetCollection<T>(colecciones[i]);
                    if (_coleccion == null)
                    {
                        dataBase.CreateCollection(colecciones[i]);
                    }
                }
            }
            catch (TimeoutException)
            {
                throw new MensajeException("Se agotó el tiempo de espera, compruebe la conexión");
            }
            catch (Exception)
            {
                throw new MensajeException("Se produjo un error al inicializar la base");
            }
           
        }
        public static async Task Create(T item, string coleccion)
        {
            try
            {
                await _client.GetDatabase(_settings.Value.DataBase).GetCollection<T>(coleccion).InsertOneAsync(item);

            }
            catch (TimeoutException)
            {
                throw new MensajeException("Se agotó el tiempo de espera, compruebe la conexión");
            }
            catch (Exception)
            {
                throw new MensajeException("Se produjo un error al registrar el elemento");
            }
        }
        public static async Task<IEnumerable<T>> GetItemsAsync(string coleccion)
        {
            try
            {
                var lista = await _database.GetCollection<T>(coleccion).Find(_ => true).ToListAsync();
                return lista;
            }
            catch (TimeoutException)
            {
                throw new MensajeException("Se agotó el tiempo de espera, compruebe la conexión");
            }
            catch (Exception)
            {
                throw new MensajeException("Se produjo un error al obtener los elementos, vuelva a intentarlo en unos minutos");
            }
        }
        public static async Task<T> GetItemAsync(string id, string coleccion)
        {
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));

            try
            {
                return await _database.GetCollection<T>(coleccion)
                                .Find(filter)
                                .FirstOrDefaultAsync();
            }
            catch (TimeoutException)
            {
                throw new MensajeException("Se agotó el tiempo de espera, compruebe la conexión");
            }
            catch (Exception)
            {
                throw new MensajeException("Se produjo un error al obtener el elemento");
            }
        }
        public static async Task<bool> DeleteAsync(ObjectId id, string coleccion)
        {
            try
            {
                DeleteResult actionResult = await _database.GetCollection<T>(coleccion).DeleteOneAsync(
                        Builders<T>.Filter.Eq("_id", id));

                return actionResult.IsAcknowledged
                    && actionResult.DeletedCount > 0;
            }
            catch (TimeoutException)
            {
                throw new MensajeException("Se agotó el tiempo de espera, compruebe la conexión");
            }
            catch (Exception)
            {
                throw new MensajeException("Se produjo un error al borrar el elemento, vuelva a intentarlo mas tarde");
            }
        }
        public static async Task<bool> UpdateAsync(ObjectId id, T item, string coleccion)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("_id", id);
                ReplaceOneResult actionResult = await _database.GetCollection<T>(coleccion).ReplaceOneAsync(filter, item, new UpdateOptions { IsUpsert = true });
                return actionResult.IsAcknowledged
                    && actionResult.ModifiedCount > 0;
            }
            catch (TimeoutException)
            {
                throw new MensajeException("Se agotó el tiempo de espera, compruebe la conexión");
            }
            catch (Exception)
            {
                throw new MensajeException("Se produjo un error al modificar el elemento, intente de nuevo mas tarde");
            }
        }
        public static async Task<T> Login(string user, string coleccion)
        {
            try
            {
                var filterBuilder = Builders<T>.Filter;
                var filter = Builders<T>.Filter.Eq("User", user);
                return await _database.GetCollection<T>(coleccion)
                                .Find(filter)
                                .FirstOrDefaultAsync();

            }
            catch(TimeoutException)
            {
                throw new MensajeException("Se agotó el tiempo de espera, compruebe la conexión");
            }
            catch (Exception)
            {
                throw new MensajeException("Ocurrió un error inesperado, vuelva a intentarlo en unos minutos");
            }
        }
        public static async Task<T> GetUsuario(string user, string coleccion)
        {
            var filter = Builders<T>.Filter.Eq("User", user);

            try
            {
                return await _database.GetCollection<T>(coleccion)
                                .Find(filter)
                                .FirstOrDefaultAsync();
            }
            catch (TimeoutException)
            {
                throw new MensajeException("Se agotó el tiempo de espera, compruebe la conexión");
            }
            catch (Exception)
            {
                throw new MensajeException("Se produjo un error al obtener al usuario, intente de nuevo mas tarde");
            }
        }
        //se deberian unificar los dos metodos para reutilizar codigo
        public static async Task<T> GetPeon(string documento, string coleccion)
        {
            var filter = Builders<T>.Filter.Eq("Documento", documento);

            try
            {
                return await _database.GetCollection<T>(coleccion)
                                .Find(filter)
                                .FirstOrDefaultAsync();
            }
            catch (TimeoutException)
            {
                throw new MensajeException("Se agotó el tiempo de espera, compruebe la conexión");
            }
            catch (Exception)
            {
                throw new MensajeException("Se produjo un error al obtener el peón. Intente de nuevo mas tarde");
            }
        }



    }
}
