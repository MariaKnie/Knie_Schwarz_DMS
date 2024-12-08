using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using System.Reflection.Metadata;
using ASP_Rest_API.SearchItems;
using Elastic.Transport;

namespace ASP_Rest_API.Services
{
    public class ElasticService : IElasticService
    {

        private readonly ElasticsearchClient _client;
        private readonly ElasticSettings _elasticSettings; 

        public ElasticService(IOptions<ElasticSettings> optionsMonitor)
        {
            _elasticSettings = optionsMonitor.Value;
            var settings = new ElasticsearchClientSettings(new Uri("http://elasticsearch:9200"))
                .Authentication(new BasicAuthentication("", ""))
                .DefaultIndex("documents");

            _client = new ElasticsearchClient(settings);
        }

        public async Task CreateIndexIfNotExistsAsync(string indexName)
        {
            if(!_client.Indices.Exists(indexName).Exists)
                await _client.Indices.CreateAsync(indexName);
        }

        public async Task<bool> AddOrUpdate(Doc document)
        {
            var response = await _client.IndexAsync(document, idx =>
            idx.Index("documents")
                .OpType(OpType.Index));
            if (!response.IsValidResponse)
            {
                // Log the error for better debugging
                Console.WriteLine($"Failed to index document: {response.DebugInformation}");
                return false;
            }

            return response.IsValidResponse;
        }

        public async Task<bool> AddOrUpdateBulk(IEnumerable<Doc> documents, string indexName)
        {
            var responce = await _client.BulkAsync(b =>
            b.Index(_elasticSettings.DefaultIndex)
                .UpdateMany(documents, (ud, u) => ud.Doc(u).DocAsUpsert(true)));

            return responce.IsValidResponse;
        }

        

        public async Task<Doc> Get(string key)
        {
            var response = await _client.GetAsync<Doc>(key, g => g.Index(_elasticSettings.DefaultIndex));

            return response.Source;
        }

        public async Task<List<Doc>> GetAll()
        {
            var response = await _client.SearchAsync<Doc>(s => s.Index(_elasticSettings.DefaultIndex));

            return response.IsValidResponse ? response.Documents.ToList() : default;
        }

        public async Task<bool> Remove(string key)
        {
            var response = await _client.DeleteAsync<Doc>(key, d => d.Index(_elasticSettings.DefaultIndex));

            return response.IsValidResponse;
        }

        public async Task<long?> RemoveAll()
        {
            var response = await _client.DeleteByQueryAsync<Doc>(d => d.Indices(_elasticSettings.DefaultIndex));

            return response.IsValidResponse ? response.Deleted : default;
            
        }
    }
}
