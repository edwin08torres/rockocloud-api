using System;
using System.Collections.Generic;
using System.Text;

namespace RockoCloud.Models.DTO
{
    public class TenantSaveDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime SubscriptionEndDate { get; set; }
        public bool Status { get; set; } = true;
        public string CreatedBy { get; set; } = string.Empty;
    }
}
