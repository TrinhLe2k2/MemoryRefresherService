using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;

namespace Location.Infrastructures.Elasticsearch
{
    public class ElasticsearchService<T> : IElasticsearchService<T> where T : class
    {
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly string _index;

        public ElasticsearchService(ElasticsearchClient elasticsearchClient, IConfiguration configuration)
        {
            _elasticsearchClient = elasticsearchClient;
            _index = configuration["Elasticsearch:ProvinceIndex"]?.ToLowerInvariant()
             ?? typeof(T).Name.ToLowerInvariant() + "s";
        }

        public async Task<string> CreateDocumentAsync(T document)
        {
            try
            {
                var response = await _elasticsearchClient.IndexAsync(document, i => i.Index(_index));
                if (!response.IsValidResponse)
                {
                    //throw new Exception("Failed to create document in Elasticsearch.");
                    return "Failed to create document in Elasticsearch.";
                }
                
                return response.Id;
            }
            catch (Exception ex)
            {
                //throw new Exception("Error creating document in Elasticsearch: " + ex.Message, ex);
                return "Error creating document in Elasticsearch: " + ex.Message;
            }
        }

        public async Task<bool> CreateDocumentsAsync(IEnumerable<T> documents)
        {
            try
            {
                if (documents == null || !documents.Any())
                    return true;

                const int batchSize = 1000;
                foreach (var batch in documents.Chunk(batchSize))
                {
                    var response = await _elasticsearchClient.BulkAsync(b =>
                    {
                        foreach (var doc in batch)
                        {
                            b.Index(doc, op => op.Index(_index));
                        }
                    });

                    if (!response.IsValidResponse)
                    {
                        //throw new Exception(response.DebugInformation);
                        return false;
                    }

                    if (response.Errors)
                    {
                        var errors = response.Items
                            .Where(i => i.Error != null)
                            .Select(i => i.Error!.Reason)
                            .Take(5);
                        //throw new Exception("Bulk errors: " + string.Join(" | ", errors));
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                //throw new Exception("Error creating documents in Elasticsearch: " + ex.Message, ex);
                return false;
            }

            //var bulkResponse = await _elasticsearchClient.BulkAsync(b => b
            //    .Index(_index)
            //    .IndexMany(documents)
            //);

            //if (!bulkResponse.IsValidResponse)
            //{
            //    throw new Exception("Failed to create documents in Elasticsearch.");
            //}

            //return !bulkResponse.Errors;
        }

        public async Task<bool> DeleteDocumentAsync(string id)
        {
            try
            {
                var response = await _elasticsearchClient.DeleteAsync<T>(id, i => i.Index(_index));
                if (!response.IsValidResponse)
                {
                    //throw new Exception("Failed to delete document from Elasticsearch.");
                    return false;
                }
                return response.Result is Result.Deleted;
            }
            catch (Exception ex)
            {
                return false;
                //throw new Exception("Error deleting document from Elasticsearch: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<T>> GetAllDocumentsAsync()
        {
            try
            {
                var searchResponse = await _elasticsearchClient.SearchAsync<T>(s => s
                    .Indices(_index)
                    .Query(q => q.MatchAll())
                    .Size(1000) // Adjust size as needed
                );

                if (!searchResponse.IsValidResponse)
                {
                    //throw new Exception($"ES search failed. Index={_index}. {searchResponse.DebugInformation}");
                    return [];
                }

                return searchResponse.Documents;
            }
            catch (Exception ex)
            {
                //throw new Exception("Error retrieving documents from Elasticsearch: " + ex.Message, ex);
                return [];
            }
        }

        public async Task<T?> GetDocumentByIdAsync(string id)
        {
            try
            {
                var response = await _elasticsearchClient.GetAsync<T>(id, i => i.Index(_index));
                if (!response.IsValidResponse || !response.Found)
                {
                    return null;
                }
                return response.Source;
            }
            catch (Exception ex)
            {
                //throw new Exception("Error retrieving document from Elasticsearch: " + ex.Message, ex);
                return null;
            }
        }

        public async Task<bool> UpdateDocumentAsync(string id, T document)
        {
            try
            {
                var response = await _elasticsearchClient.IndexAsync(document, i => i.Index(_index).Id(id));
                if (!response.IsValidResponse)
                {
                    throw new Exception("Failed to update document in Elasticsearch.");
                }
                return response.Result is Result.Updated or Result.Created;
            }
            catch (Exception ex)
            {
                //throw new Exception("Error updating document in Elasticsearch: " + ex.Message, ex);
                return false;
            }
        }
    }
}
