using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using ProyectoTransportesAndes.Configuracion;
using Microsoft.Extensions.Options;

namespace ProyectoTransportesAndes.Persistencia
{
    public static class DBRepository<T>
    {
        private static IOptions<AppSettings> _settings;
        private static DocumentClient client;
        private static string[] colecciones = {"Viajes","Administradores","Administrativos","Camiones","Camionetas","Choferes","Clientes","Peones","Tarifas","Vehiculos"};
        public static void Iniciar(IOptions<AppSettings>settings)
        {
            _settings = settings;
            client = new Microsoft.Azure.Documents.Client.DocumentClient(new Uri(_settings.Value.Endpoint), _settings.Value.AuthKey);
            CreateDataBaseIfNotExistsAsync().Wait();
            CreateCollectionsIfNotExistsAsync().Wait();
        }
        private static async Task CreateDataBaseIfNotExistsAsync() {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_settings.Value.DataBase));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = _settings.Value.DataBase });
                }
                else
                {
                    throw;
                }
            }
        }
        private static async Task CreateCollectionsIfNotExistsAsync() {
            for (int i = 0; i < 10; i++) {
                try
                {
                    await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_settings.Value.DataBase, colecciones[i]));
                }
                catch (DocumentClientException e)
                {
                    if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        await client.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(_settings.Value.DataBase), new DocumentCollection { Id = colecciones[i] }, new RequestOptions { OfferThroughput = 1000 });
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
        public static async Task<Document> Create(T item, string coleccion)
        {
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_settings.Value.DataBase,coleccion),item);
        }
        public static async Task<IEnumerable<T>> GetItemsAsync(string coleccion)
        {
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(UriFactory.CreateDocumentCollectionUri(_settings.Value.DataBase, coleccion)).AsDocumentQuery();
            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }
            return results;
        }
        public static async Task<T> GetItemsAsync(string id, string coleccion)
        {
            try
            {
                Document document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(_settings.Value.DataBase, coleccion, id));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return default(T);
                }
                else
                {
                    throw;
                }
            }
        }
        public static async Task<Document> UpdateItemsAsync(string id, T item,string coleccion)
        {
            return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(_settings.Value.DataBase, coleccion, id), item);
        }
        public static async Task<Document> DeleteItemsAsync(string id, string coleccion)
        {
            return await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_settings.Value.DataBase, coleccion, id));
        }

    }
}
