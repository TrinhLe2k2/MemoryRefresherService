using Location.Models;
using Location.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Location.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProvincesController : ControllerBase
    {
        private readonly IProvinceService _provinceService;

        public ProvincesController(IProvinceService provinceService)
        {
            _provinceService = provinceService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(InsertProvinceRequest req)
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
                var result = await _provinceService.GetProvince(id);
                if (result == null)
                {
                    return NotFound();
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
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-weather")]
        public IActionResult GetList()
        {
            var weather = new [] { "Sunny", "Cloudy", "Rainy", "Windy", "Stormy"};
            return Ok(weather);
        }
    }
}
