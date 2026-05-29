using Microsoft.EntityFrameworkCore;
using RockoCloud.DataAccess.Context;
using RockoCloud.DataAccess.Interfaces;
using RockoCloud.Models.DTO;
using RockoCloud.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RockoCloud.DataAccess.Repositories
{
    public class TenantRepository(RockoCloudContext context) : ITenantRepository
    {
        public async Task<Tenant?> GetTenantByCodeAsync(string code)
        {
            return await context.Tenant
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<IEnumerable<Tenant>> GetTenantsAsync()
        {
            return await context.Tenant.OrderBy(t => t.Name).ToListAsync();
        }

        public async Task<Tenant> SaveTenantAsync(Tenant tenant)
        {
            await context.Tenant.AddAsync(tenant);
            await context.SaveChangesAsync();

            return tenant;
        }

    }
}
