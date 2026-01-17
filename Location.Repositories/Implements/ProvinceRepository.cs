using Location.Infrastructures;
using Location.Models;
using Location.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Location.Repositories.Implements
{
    public class ProvinceRepository : RepositoryBase, IProvinceRepository
    {
        public ProvinceRepository(IDbConnectionFactory factory) : base(factory)
        {
        }

        public async Task<int> CreateProvince(CreateProvince model)
        {
            try
            {
                var result = await base.QueryFirstOrDefaultAsync<int>("SP_Provinces_Create", new
                {
                    @Code = model.Code,
                    @Name = model.Name,
                    @User = model.User
                });
                return result;
            }
            catch (Exception) { throw; }
        }

        public async Task<int> UpdateProvince(UpdateProvince model)
        {
            try
            {
                var result = await base.ExecuteAsync("SP_Provinces_Update", new
                {
                    @Id = model.Id,
                    @Code = model.Code,
                    @Name = model.Name,
                    @User = model.User
                });
                return result;
            }
            catch (Exception) { throw; }
        }

        public async Task<int> DeleteProvince(int id)
        {
            try
            {
                var result = await base.ExecuteAsync("SP_Provinces_Delete", new
                {
                    @Id = id
                });
                return result;
            }
            catch (Exception) { throw; }
        }

        public async Task<DetailProvince?> GetProvince(int id)
        {
            try
            {
                var result = await base.QueryFirstOrDefaultAsync<DetailProvince>("SP_Provinces_Detail", new
                {
                    @Id = id
                });
                return result;
            }
            catch (Exception) { throw; }
        }

        public async Task<IEnumerable<DetailProvince>> GetProvinces(string? keyword)
        {
            try
            {
                var result = await base.QueryAsync<DetailProvince>("SP_Provinces_List", new
                {
                    @Keyword = keyword
                });
                return result;
            }
            catch (Exception) { throw; }
        }
    }
}
