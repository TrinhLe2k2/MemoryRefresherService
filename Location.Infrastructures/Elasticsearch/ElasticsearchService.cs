using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;

namespace Location.Infrastructures.Elasticsearch
{
    public class ElasticsearchService<T> : IElasticsearchService<T> where T : class
    {
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly ILogger<ElasticsearchService<T>> _logger;
        private readonly string _index;

        public ElasticsearchService(ElasticsearchClient elasticsearchClient, ILogger<ElasticsearchService<T>> logger, IConfiguration configuration)
        {
            _elasticsearchClient = elasticsearchClient;
            _index = configuration["Elasticsearch:ProvinceIndex"]?.ToLowerInvariant() ?? typeof(T).Name.ToLowerInvariant() + "s";
            _logger = logger;
        }

        public async Task<string> CreateDocumentAsync(T document)
        {
            try
            {
                var response = await _elasticsearchClient.IndexAsync(document, i => i.Index(_index));
                if (!response.IsValidResponse)
                {
                    _logger.LogError("Failed to create document in Elasticsearch. Index: {Index}, DebugInfo: {DebugInfo}", _index, response.DebugInformation);
                    throw new Exception("Failed to create document in Elasticsearch.");
                }
                _logger.LogInformation("Document created in Elasticsearch with ID: {Id}", response.Id);
                return response.Id;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating document in Elasticsearch: " + ex.Message, ex);
            }
        }

        public async Task<bool> CreateDocumentsAsync(IEnumerable<T> documents)
        {
            try
            {
                if (documents == null || !documents.Any())
                {
                    _logger.LogWarning("No documents provided for bulk creation in Elasticsearch.");
                    return true;
                }

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
                        _logger.LogError("Failed to create documents in Elasticsearch. Index: {Index}, DebugInfo: {DebugInfo}", _index, response.DebugInformation);
                        throw new Exception("Failed to create documents in Elasticsearch.");
                    }

                    if (response.Errors)
                    {
                        var errors = response.Items
                            .Where(i => i.Error != null)
                            .Select(i => i.Error!.Reason)
                            .Take(5);

                        _logger.LogError("Bulk errors occurred while creating documents in Elasticsearch: {Errors}", string.Join(" | ", errors));
                        throw new Exception("Bulk errors occurred while creating documents in Elasticsearch.");
                    }
                }
                _logger.LogInformation("Successfully created {Count} documents in Elasticsearch index {Index}.", documents.Count(), _index);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating documents in Elasticsearch.");
                throw new Exception("Error creating documents in Elasticsearch: " + ex.Message, ex);
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
                    _logger.LogError("Failed to delete document in Elasticsearch. Index: {Index}, DebugInfo: {DebugInfo}", _index, response.DebugInformation);
                    throw new Exception("Failed to delete document in Elasticsearch.");
                }
                _logger.LogInformation("Document with ID {Id} deleted from Elasticsearch index {Index}.", id, _index);
                return response.Result is Result.Deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document from Elasticsearch.");
                throw new Exception("Error deleting document from Elasticsearch: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<T>> GetAllDocumentsAsync(string? keyword)
        {
            try
            {
                if (!await IndexExistsAsync())
                {
                    _logger.LogWarning("Elasticsearch index {Index} does not exist.", _index);
                    return [];
                }
                var searchResponse = await _elasticsearchClient.SearchAsync<T>(s => s
                    .Indices(_index)
                    .Query(q =>
                    {
                        if (string.IsNullOrEmpty(keyword))
                        {
                            q.MatchAll();
                        }
                        else
                        {
                            q.MultiMatch(m => m
                                .Fields("*")
                                .Query(keyword)
                            );
                        }
                    })
                    .Size(1000) // Adjust size as needed
                );

                if (!searchResponse.IsValidResponse)
                {
                    _logger.LogError("Failed to search documents in Elasticsearch. Index: {Index}, DebugInfo: {DebugInfo}", _index, searchResponse.DebugInformation);
                    throw new Exception("Failed to search documents in Elasticsearch.");
                }
                _logger.LogInformation("Retrieved {Count} documents from Elasticsearch index {Index} with keyword '{Keyword}'.", searchResponse.Documents.Count, _index, keyword);
                return searchResponse.Documents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents from Elasticsearch.");
                throw new Exception("Error retrieving documents from Elasticsearch: " + ex.Message, ex);
            }
        }

        public async Task<T?> GetDocumentByIdAsync(string id)
        {
            try
            {
                var response = await _elasticsearchClient.GetAsync<T>(id, i => i.Index(_index));
                if (!response.IsValidResponse || !response.Found)
                {
                    _logger.LogWarning("Document with ID {Id} not found in Elasticsearch index {Index}.", id, _index);
                    return null;
                }
                _logger.LogInformation("Document with ID {Id} retrieved from Elasticsearch index {Index}.", id, _index);
                return response.Source;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document with ID {Id} from Elasticsearch.", id);
                throw new Exception("Error retrieving document from Elasticsearch: " + ex.Message, ex);
            }
        }

        public async Task<bool> UpdateDocumentAsync(string id, T document)
        {
            try
            {
                var response = await _elasticsearchClient.IndexAsync(document, i => i.Index(_index).Id(id));
                if (!response.IsValidResponse)
                {
                    _logger.LogError("Failed to update document in Elasticsearch. Index: {Index}, DebugInfo: {DebugInfo}", _index, response.DebugInformation);
                    throw new Exception("Failed to update document in Elasticsearch.");
                }
                _logger.LogInformation("Document with ID {Id} updated in Elasticsearch index {Index}.", id, _index);
                return response.Result is Result.Updated or Result.Created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document in Elasticsearch.");
                throw new Exception("Error updating document in Elasticsearch: " + ex.Message, ex);
            }
        }

        public async Task<bool> IndexExistsAsync()
        {
            try
            {
                var response = await _elasticsearchClient.Indices.ExistsAsync(_index);

                return response.Exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check Elasticsearch index existence. Index={Index}", _index);
                return false;
            }
        }

    }
}
