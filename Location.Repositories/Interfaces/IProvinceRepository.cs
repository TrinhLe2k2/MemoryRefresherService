using Location.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Location.Repositories.Interfaces
{
    public interface IProvinceRepository
    {
        Task<int> CreateProvince(InsertProvince model);
        Task<int> UpdateProvince(UpdateProvince model);
        Task<int> DeleteProvince(int id);
        Task<DetailProvince?> GetProvince(int id);
        Task<IEnumerable<DetailProvince>> GetProvinces(string? keyword);
    }
}
