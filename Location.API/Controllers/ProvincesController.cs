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
                if (result < 1)
                {
                    return BadRequest("Create province failed");
                }
                
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
                if (result < 1)
                {
                    return BadRequest("Update province failed");
                }
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
                if (result < 1)
                {
                    return BadRequest("Delete province failed");
                }
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
                var result = await _provinceService.GetProvince(id);
                if (result is null)
                {
                    return NotFound("Province not found");
                }
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
                var result = await _provinceService.GetProvinces(keyword);
                if (!result.Any())
                {
                    return NotFound("No provinces found");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
