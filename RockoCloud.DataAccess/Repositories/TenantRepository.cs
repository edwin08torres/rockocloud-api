using Microsoft.EntityFrameworkCore;
using RockoCloud.DataAccess.Context;
using RockoCloud.DataAccess.Interfaces;
using RockoCloud.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RockoCloud.DataAccess.Repositories
{
    public class TenantRepository(RockoCloudContext context) : ITenantRepository
    {
        public Task<Tenant?> GetTenantByCodeAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Tenant>> GetTenantsAsync()
        {
            return await context.Tenant.OrderBy(t => t.Name).ToListAsync();
        }
    }
}
