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
        private readonly ILogger _logger;
        private readonly IRedisCacheUsingDistributed _redisCacheInMemory;

        public ProvincesController(IProvinceService provinceService, IRedisCacheUsingMultiplexer cache, ILogger<ProvincesController> logger, IRedisCacheUsingDistributed redisCacheInMemory)
        {
            _provinceService = provinceService;
            _cache = cache;
            _logger = logger;
            _redisCacheInMemory = redisCacheInMemory;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProvinceRequest req)
        {
            try
            {
                var model = req.ToModel("system");
                var result = await _provinceService.CreateProvince(model);
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

                var result = await _provinceService.GetProvince(id);
                if (result == null)
                {
                    return NotFound();
                }
                await _redisCacheInMemory.SetAsync(key, result, TimeSpan.FromMinutes(3));

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

                var result = await _provinceService.GetProvinces(keyword);
                await _cache.SetAsync(key, result, TimeSpan.FromMinutes(3));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
