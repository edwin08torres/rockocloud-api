using Microsoft.EntityFrameworkCore;
using RockoCloud.Models.Entities;

namespace RockoCloud.DataAccess.Context
{
    public class RockoCloudContext(DbContextOptions<RockoCloudContext> options) : DbContext(options)
    {
        public DbSet<Tenant> Tenant { get; set; }
        public DbSet<Branch> Branch { get; set; }
    }
}
