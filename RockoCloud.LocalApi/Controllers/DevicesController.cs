using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RockoCloud.DataAccess;
using RockoCloud.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RockoCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public DevicesController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
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
        var device = await _context.Devices
            .Include(d => d.Branch)
            .FirstOrDefaultAsync(d => d.PairingPin == request.Pin);

        if (device == null) return NotFound("PIN inválido o expirado.");

        device.DeviceKey = Guid.NewGuid().ToString("N");
        device.PairingPin = null;
        device.IsActive = true;

        await _context.SaveChangesAsync();

        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("DeviceId", device.Id.ToString()),
                new Claim("TenantId", device.Branch.TenantId.ToString()),
                new Claim(ClaimTypes.Role, "RockolaDevice")
            }),
            Expires = DateTime.UtcNow.AddYears(10),
            Issuer = _configuration["Jwt:Issuer"], 
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtString = tokenHandler.WriteToken(token);

        return Ok(new
        {
            token = jwtString,
            deviceKey = device.DeviceKey,
            message = "¡Vinculación exitosa!",
            systemName = device.Name
        });
    }
}

public record RegisterDeviceRequest(string Name, Guid BranchId);
public record PairRequest(string Pin);