using AutoMapper;
using RockoCloud.BusinessLogic.Interfaces;
using RockoCloud.DataAccess.Interfaces;
using RockoCloud.Models.DTO;
using RockoCloud.Models.Entities;
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
            var tenant = await tenantRepository.GetTenantByCodeAsync(code);

            return mapper.Map<TenantDTO?>(tenant);
        }

        public async Task<TenantDTO> SaveTenantAsync(TenantSaveDTO tenantSaveDTO)
        {
            var tenantExist = await tenantRepository.GetTenantByCodeAsync(tenantSaveDTO.Code);

            if (tenantExist is not null)
            {
                throw new Exception("Ya existe un tenant con ese código.");
            }

            var tenant = mapper.Map<Tenant>(tenantSaveDTO);

            tenant.TenantID = Guid.NewGuid();
            tenant.CreatedAt = DateTime.Now;
            tenant.UpdatedAt = DateTime.Now;
            tenant.UpdatedBy = tenantSaveDTO.CreatedBy;

            var tenantSaved = await tenantRepository.SaveTenantAsync(tenant);

            return mapper.Map<TenantDTO>(tenantSaved);
        }
    }
}
