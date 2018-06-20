using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ProyectoTransportesAndes.Persistencia
{
    public static class DBRepositoryMongo<T>
    {
        private static IOptions<AppSettingsMongo> _settings;
        private static IMongoDatabase _database = null;
        private static string[] colecciones = { "Viajes", "Administradores", "Administrativos", "Camiones", "Camionetas", "Choferes", "Clientes", "Peones"};
        private static MongoClient _client;

        public static void Iniciar(IOptions<AppSettingsMongo>settings)
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
                    //if (_database.GetCollection<PruebaMongo>("Prueba") == null)
                    //{
                    //    _database.CreateCollection("Prueba");
                    //}
                }
            }
        }
        public static void CrearColecciones(IMongoDatabase dataBase)
        {
            for (int i = 0; i < 8; i++)
            {
                IMongoCollection<T> _coleccion = dataBase.GetCollection<T>(colecciones[i]);
                if (_coleccion== null)
                {
                    dataBase.CreateCollection(colecciones[i]);
                }
            }
        }
        public static async Task Create(T item, string coleccion)
        {
            await _client.GetDatabase(_settings.Value.DataBase).GetCollection<T>(coleccion).InsertOneAsync(item);
        }
        public static async Task<IEnumerable<T>> GetItemsAsync(string coleccion)
        {
            try
            {
                return await _database.GetCollection<T>(coleccion).Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
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
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
        public static async Task<bool> DeleteAsync(ObjectId id,string coleccion)
        {
            try
            {
                DeleteResult actionResult = await _database.GetCollection<T>(coleccion).DeleteOneAsync(
                        Builders<T>.Filter.Eq("_id", id));

                return actionResult.IsAcknowledged
                    && actionResult.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
        public static async Task<bool> UpdateAsync(ObjectId id, T item,string coleccion)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("_id", id);
                ReplaceOneResult actionResult = await _database.GetCollection<T>(coleccion).ReplaceOneAsync(filter,item,new UpdateOptions {IsUpsert=true });
                return actionResult.IsAcknowledged
                    && actionResult.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
        public static async Task<T> Login(string user,string pass)
        {
            var filterBuilder = Builders<T>.Filter;
             var filter = Builders<T>.Filter.Eq("User", user);
            //var filter = filterBuilder.Eq("User", user) & filterBuilder.Eq("Password", pass);
           

            try
            {
                return await _database.GetCollection<T>("Usuarios")
                                .Find(filter)
                                .FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
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
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
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
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

    }
}
