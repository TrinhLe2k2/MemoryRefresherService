using Location.Infrastructures.Elasticsearch;
using Location.Infrastructures.Redis;
using Location.Models;
using Location.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace Location.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProvincesController : ControllerBase
    {
        private readonly IProvinceService _provinceService;
        private readonly IRedisCacheUsingMultiplexer _cache;
        private readonly IRedisCacheUsingDistributed _redisCacheInMemory;
        private readonly IElasticsearchService<DetailProvince> _elasticsearchService;

        public ProvincesController(IProvinceService provinceService, IRedisCacheUsingMultiplexer cache, IRedisCacheUsingDistributed redisCacheInMemory, IElasticsearchService<DetailProvince> elasticsearchService)
        {
            _provinceService = provinceService;
            _cache = cache;
            _redisCacheInMemory = redisCacheInMemory;
            _elasticsearchService = elasticsearchService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProvinceRequest req)
        {
            try
            {
                var model = req.ToModel("system");
                var result = await _provinceService.CreateProvince(model);
                if (result < 0)
                {
                    return BadRequest("Create province failed");
                }
                var detail = await _provinceService.GetProvince(result);
                if (detail is null)
                {
                    return NotFound();
                }
                var esResult = await _elasticsearchService.CreateDocumentAsync(detail);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPut]
        public async Task<IActionResult> Update(UpdateProvinceRequest req)
        {
            try
            {
                var model = req.ToModel("system");
                var result = await _provinceService.UpdateProvince(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _provinceService.DeleteProvince(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var key = $"province:{id}";
                var cached = await _redisCacheInMemory.GetAsync<DetailProvince>(key);
                if (cached != null) return Ok(cached);

                var esGetByIdResult = await _elasticsearchService.GetDocumentByIdAsync(id.ToString());
                if (esGetByIdResult is not null)
                {
                    await _redisCacheInMemory.SetAsync(key, esGetByIdResult, TimeSpan.FromMinutes(3));
                    return Ok(esGetByIdResult);
                }

                var result = await _provinceService.GetProvince(id);
                if (result is null)
                {
                    return NotFound();
                }
                await _redisCacheInMemory.SetAsync(key, result, TimeSpan.FromMinutes(3));

                var esCreateResult = await _elasticsearchService.CreateDocumentAsync(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] string? keyword)
        {
            try
            {
                var key = $"provinces:{keyword}";

                var cached = await _cache.GetAsync<IEnumerable<DetailProvince>>(key);
                if (cached != null) return Ok(cached);

                var esGetallResult = await _elasticsearchService.GetAllDocumentsAsync();
                if (esGetallResult.Any())
                {
                    await _cache.SetAsync(key, esGetallResult, TimeSpan.FromMinutes(3));
                    return Ok(esGetallResult);
                }

                var result = await _provinceService.GetProvinces(keyword);
                await _cache.SetAsync(key, result, TimeSpan.FromMinutes(3));
                var multiEsCreateResult = await _elasticsearchService.CreateDocumentsAsync(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
