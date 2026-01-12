using Location.Models;
using Location.Repositories.Interfaces;
using Location.Services.Interfaces;
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

        public ProvinceService(IProvinceRepository provinceRepository)
        {
            _provinceRepository = provinceRepository;
        }

        public async Task<int> CreateProvince(CreateProvince model)
        {
            try
            {
                return await _provinceRepository.CreateProvince(model);
            }
            catch (Exception) { throw; }
        }

        public async Task<int> UpdateProvince(UpdateProvince model)
        {
            try
            {
                return await _provinceRepository.UpdateProvince(model);
            }
            catch (Exception) { throw; }
        }

        public async Task<int> DeleteProvince(int id)
        {
            try
            {
                return await _provinceRepository.DeleteProvince(id);
            }
            catch (Exception) { throw; }
        }

        public async Task<DetailProvince?> GetProvince(int id)
        {
            try
            {
                return await _provinceRepository.GetProvince(id);
            }
            catch (Exception) { throw; }
        }

        public async Task<IEnumerable<DetailProvince>> GetProvinces(string? keyword)
        {
            try
            {
                return await _provinceRepository.GetProvinces(keyword);
            }
            catch (Exception) { throw; }
        }
    }
}
