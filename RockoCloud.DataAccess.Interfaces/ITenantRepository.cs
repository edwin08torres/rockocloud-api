using RockoCloud.Models.DTO;
using RockoCloud.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RockoCloud.DataAccess.Interfaces
{
    public interface ITenantRepository
    {
        Task<IEnumerable<Tenant>> GetTenantsAsync();
        Task<Tenant?> GetTenantByCodeAsync(string code);
        Task<Tenant> SaveTenantAsync(Tenant tenant);
    }
}
