public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime SubscriptionEndDate { get; set; }
    public string Status { get; set; } = "Active";
    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<User> Users { get; set; } = new List<User>();
}

public class Branch
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Device> Devices { get; set; } = new List<Device>();
}

public class Device
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DeviceKey { get; set; } = string.Empty; 
    public string? PairingPin { get; set; }
    public bool IsActive { get; set; }
    public Branch Branch { get; set; } = null!;
}
public class User
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Admin";
    public Tenant Tenant { get; set; } = null!;
}