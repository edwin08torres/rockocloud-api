using RockoCloud.Models.DTO;
using RockoCloud.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RockoCloud.BusinessLogic.Interfaces
{
    public interface ITenantService
    {
        Task<IEnumerable<TenantDTO>> GetTenantsAsync();
        Task<TenantDTO?> GetTenantByCodeAsync(string code);
        Task<TenantDTO> SaveTenantAsync(TenantSaveDTO tenantSaveDTO);
    }
}
