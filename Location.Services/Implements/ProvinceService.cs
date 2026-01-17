using Elastic.Clients.Elasticsearch;
using Location.Infrastructures.Elasticsearch;
using Location.Infrastructures.Redis;
using Location.Models;
using Location.Repositories.Interfaces;
using Location.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Location.Services.Implements
{
    public class ProvinceService : IProvinceService
    {
        private readonly IProvinceRepository _provinceRepository;
        private readonly IRedisCacheUsingMultiplexer _cache;
        private readonly IElasticsearchService<DetailProvince> _elasticsearchService;
        private readonly ILogger<ProvinceService> _logger;

        public ProvinceService(IProvinceRepository provinceRepository, IRedisCacheUsingMultiplexer cache, IElasticsearchService<DetailProvince> elasticsearchService, ILogger<ProvinceService> logger)
        {
            _provinceRepository = provinceRepository;
            _cache = cache;
            _elasticsearchService = elasticsearchService;
            _logger = logger;
        }

        public async Task<int> CreateProvince(CreateProvince model)
        {
            try
            {
                var dbResult = await _provinceRepository.CreateProvince(model);
                if (dbResult < 1)
                {
                    return dbResult;
                }

                var detailEntity = await GetProvince(dbResult);
                if (detailEntity is null)
                {
                    return dbResult;
                }

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _elasticsearchService.CreateDocumentAsync(detailEntity);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to sync new province to Elasticsearch. ProvinceId={ProvinceId}", dbResult);
                    }
                });

                return dbResult;
            }
            catch (Exception)
            {
                _logger.LogError("Error creating province.");
                throw new Exception("Error creating province.");
            }
        }

        public async Task<int> UpdateProvince(UpdateProvince model)
        {
            try
            {
                var dbResult = await _provinceRepository.UpdateProvince(model);
                if (dbResult < 1)
                {
                    return dbResult;
                }
                var detailEntity = new DetailProvince
                {
                    Id = model.Id,
                    Code = model.Code,
                    Name = model.Name,
                };
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _elasticsearchService.UpdateDocumentAsync(detailEntity.Id.ToString(), detailEntity);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to sync updated province to Elasticsearch. ProvinceId={ProvinceId}", model.Id);
                    }
                });
                return dbResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating province. ProvinceId={ProvinceId}", model.Id);
                throw new Exception("Error updating province: " + ex.Message, ex);
            }
        }

        public async Task<int> DeleteProvince(int id)
        {
            try
            {
                var dbResult = await _provinceRepository.DeleteProvince(id);
                if (dbResult < 1)
                {
                    return dbResult;
                }

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _elasticsearchService.DeleteDocumentAsync(id.ToString());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to sync deleted province to Elasticsearch. ProvinceId={ProvinceId}", id);
                    }
                });

                return dbResult;
            }
            catch (Exception) { throw; }
        }

        public async Task<DetailProvince?> GetProvince(int id)
        {
            try
            {
                var esResult = await _elasticsearchService.GetDocumentByIdAsync(id.ToString());
                if (esResult is not null)
                {
                    return esResult;
                }

                var dbResult = await _provinceRepository.GetProvince(id);
                if (dbResult is not null)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _elasticsearchService.CreateDocumentAsync(dbResult);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to sync province to Elasticsearch. ProvinceId={ProvinceId}", id);
                        }
                    });
                }

                return dbResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving province. ProvinceId={ProvinceId}", id);
                throw new Exception("Error retrieving province: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<DetailProvince>> GetProvinces(string? keyword)
        {
            var cacheKey = $"provinces:{keyword}";
            try
            {
                // 1️. Redis
                var cached = await _cache.GetAsync<IEnumerable<DetailProvince>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogDebug("Provinces cache hit. keyword={keyword}", keyword);
                    return cached;
                }

                // 2️. Elasticsearch
                var esResult = await _elasticsearchService.GetAllDocumentsAsync(keyword);
                if (esResult.Any())
                {
                    _logger.LogInformation("Provinces found in Elasticsearch. keyword={keyword}", keyword);

                    await _cache.SetAsync(cacheKey, esResult, TimeSpan.FromMinutes(3));
                    return esResult;
                }

                // 3️. Database
                var dbResult = await _provinceRepository.GetProvinces(keyword);

                if (!dbResult.Any())
                {
                    _logger.LogWarning("Provinces not found. keyword={keyword}", keyword);
                    return [];
                }

                // 4️. Sync cache + ES (best-effort)
                await _cache.SetAsync(cacheKey, dbResult, TimeSpan.FromMinutes(3));

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _elasticsearchService.CreateDocumentsAsync(dbResult);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to sync province to Elasticsearch. keyword={keyword}", keyword);
                    }
                });

                return dbResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving provinces. keyword={keyword}", keyword);
                throw new Exception("Error retrieving provinces: " + ex.Message, ex);
            }
        }
    }
}
