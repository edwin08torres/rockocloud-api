using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RockoCloud.DataAccess;
using RockoCloud.Models;

namespace RockoCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TenantsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTenants()
    {
        var role = User.FindFirst("Role")?.Value;
        if (role != "SuperAdmin") return Forbid();

        var tenants = await _context.Tenants
            .Select(t => new {
                t.Id,
                t.Name,
                t.SubscriptionEndDate,
                t.Status,
                IsExpired = t.SubscriptionEndDate < DateTime.UtcNow
            })
            .ToListAsync();

        return Ok(tenants);
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterTenant([FromBody] CreateTenantRequest request)
    {
        var role = User.FindFirst("Role")?.Value;
        if (role != "SuperAdmin") return Forbid();

        var endDate = DateTime.UtcNow.AddMonths(request.Months);

        var newTenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.BusinessName,
            SubscriptionEndDate = endDate,
            Status = "Active"
        };

        _context.Tenants.Add(newTenant);

        var newAdmin = new User
        {
            Id = Guid.NewGuid(),
            TenantId = newTenant.Id,
            Email = request.AdminEmail,
            PasswordHash = request.AdminPassword,
            Role = "Admin"
        };

        _context.Users.Add(newAdmin);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Cliente y usuario administrador creados", TenantId = newTenant.Id });
    }

    [HttpPost("{id}/renew")]
    public async Task<IActionResult> RenewSubscription(Guid id, [FromBody] RenewRequest request)
    {
        var role = User.FindFirst("Role")?.Value;
        if (role != "SuperAdmin") return Forbid();

        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null) return NotFound();

        var baseDate = tenant.SubscriptionEndDate > DateTime.UtcNow ? tenant.SubscriptionEndDate : DateTime.UtcNow;
        tenant.SubscriptionEndDate = baseDate.AddMonths(request.Months);
        tenant.Status = "Active";

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Renovado", EndsAt = tenant.SubscriptionEndDate });
    }

    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> SuspendTenant(Guid id)
    {
        var role = User.FindFirst("Role")?.Value;
        if (role != "SuperAdmin") return Forbid();

        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null) return NotFound();

        tenant.Status = "Suspended";
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Suspendido" });
    }

    [HttpPost("{id}/block")]
    public async Task<IActionResult> BlockTenant(Guid id)
    {
        var role = User.FindFirst("Role")?.Value;
        if (role != "SuperAdmin") return Forbid();

        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null) return NotFound();

        tenant.Status = "Blocked";
        tenant.SubscriptionEndDate = DateTime.UtcNow.AddDays(-1);

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Bloqueado" });
    }
}

public record CreateTenantRequest(string BusinessName, int Months, string AdminEmail, string AdminPassword);
public record RenewRequest(int Months);