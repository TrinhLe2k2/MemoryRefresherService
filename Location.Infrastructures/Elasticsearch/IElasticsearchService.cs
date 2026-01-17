using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Location.Infrastructures.Elasticsearch
{
    public interface IElasticsearchService<T> where T : class
    {
        Task<string> CreateDocumentAsync(T document);
        Task<bool> CreateDocumentsAsync(IEnumerable<T> documents);
        Task<T?> GetDocumentByIdAsync(string id);
        Task<bool> UpdateDocumentAsync(string id, T document);
        Task<bool> DeleteDocumentAsync(string id);
        Task<IEnumerable<T>> GetAllDocumentsAsync(string? keyword);
    }
}
