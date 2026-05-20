using AutoMapper;
using RockoCloud.BusinessLogic.Interfaces;
using RockoCloud.DataAccess.Interfaces;
using RockoCloud.Models.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace RockoCloud.BusinessLogic.Services
{
    public class TenantService(
        ITenantRepository tenantRepository,
        IMapper mapper) : ITenantService
    {

        public async Task<IEnumerable<TenantDTO>> GetTenantsAsync()
        {
            var tenants = await tenantRepository.GetTenantsAsync();

            return mapper.Map<IEnumerable<TenantDTO>>(tenants);
        }

        public async Task<TenantDTO?> GetTenantByCodeAsync(string code)
        {
            throw new NotImplementedException();
        }
    }
}
