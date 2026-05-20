using RockoCloud.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockoCloud.Models.DTO
{
    public class TenantDTO
    {
        public Guid TenantID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime SubscriptionEndDate { get; set; }
        public string Status { get; set; } = "Active";
        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
        //public ICollection<User> Users { get; set; } = new List<User>();
    }
}
