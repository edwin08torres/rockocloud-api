using RockoCloud.Models.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace RockoCloud.BusinessLogic.Interfaces
{
    public interface ITenantService
    {
        Task<IEnumerable<TenantDTO>> GetTenantsAsync();
        Task<TenantDTO?> GetTenantByCodeAsync(string code);
    }
}
