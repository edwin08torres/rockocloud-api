using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RockoCloud.DataAccess;
using RockoCloud.Models;
using System.Security.Claims;

namespace RockoCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DevicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyDevices()
    {
        var tenantId = Guid.Parse(User.FindFirstValue("TenantId")!);
        var role = User.FindFirstValue("Role");

        var query = _context.Devices.AsQueryable();

        if (role != "SuperAdmin")
        {
            query = query.Where(d => d.Branch.TenantId == tenantId);
        }

        var devices = await query
            .Select(d => new
            {
                id = d.Id,
                name = d.Name,
                isActive = d.IsActive,
                pairingPin = d.PairingPin,
                branch = new
                {
                    name = d.Branch.Name,
                    tenant = new { name = d.Branch.Tenant.Name }
                }
            })
            .ToListAsync();

        return Ok(devices);
    }

    [HttpGet("branches")]
    public async Task<IActionResult> GetMyBranches()
    {
        var tenantId = Guid.Parse(User.FindFirstValue("TenantId")!);
        var branches = await _context.Branches
            .Where(b => b.TenantId == tenantId)
            .Select(b => new { b.Id, b.Name })
            .ToListAsync();

        return Ok(branches);
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterPlaceholder([FromBody] RegisterDeviceRequest request)
    {
        var tenantId = Guid.Parse(User.FindFirstValue("TenantId")!);
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.BranchId);
        if (branch == null || branch.TenantId != tenantId) return BadRequest("Sucursal no válida.");

        var pin = new Random().Next(100000, 999999).ToString();

        var device = new Device
        {
            Id = Guid.NewGuid(),
            BranchId = request.BranchId,
            Name = request.Name,
            PairingPin = pin,
            IsActive = false,
            DeviceKey = ""
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        return Ok(new { DeviceId = device.Id, Pin = pin });
    }

    [HttpPost("pair")]
    [AllowAnonymous]
    public async Task<IActionResult> PairDevice([FromBody] PairRequest request)
    {
        var device = await _context.Devices.FirstOrDefaultAsync(d => d.PairingPin == request.Pin);

        if (device == null) return NotFound("PIN inválido o expirado.");

        device.DeviceKey = Guid.NewGuid().ToString("N");
        device.PairingPin = null;
        device.IsActive = true;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            DeviceKey = device.DeviceKey,
            Message = "¡Vinculación exitosa!",
            SystemName = device.Name
        });
    }
}

public record RegisterDeviceRequest(string Name, Guid BranchId);
public record PairRequest(string Pin);