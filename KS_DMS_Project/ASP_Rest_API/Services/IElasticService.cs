using ASP_Rest_API.SearchItems;
using System.Reflection.Metadata;

namespace ASP_Rest_API.Services
{
    public interface IElasticService
    {
        // create index
        Task CreateIndexIfNotExistsAsync(string indexName);

        // add or update document
        Task<bool> AddOrUpdate(Doc document);

        // add or update document bulk
        Task<bool> AddOrUpdateBulk(IEnumerable<Doc> documents, string indexName);

        // Get document
        Task<Doc> Get(string key);

        // get all documents
        Task<List<Doc>?> GetAll();

        // remove
        Task<bool> Remove(string key);

        Task<long?> RemoveAll();
    }
}
