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
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || user.PasswordHash != request.Password)
        {
            return Unauthorized(new { message = "Credenciales incorrectas" });
        }

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            Token = token,
            User = new
            {
                user.Id,
                user.Email,
                user.Role,
                TenantId = user.TenantId,
                TenantName = user.Tenant.Name
            }
        });
    }

    [HttpPost("seed-admin")]
    public async Task<IActionResult> SeedAdmin()
    {
        if (await _context.Users.AnyAsync())
            return BadRequest("Ya existen usuarios en la base de datos.");

        var masterTenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "DETDevs Master",
            Code = "DET-MASTER"
        };

        var masterUser = new User
        {
            Id = Guid.NewGuid(),
            TenantId = masterTenant.Id,
            Email = "admin@detdevs.com",
            PasswordHash = "admin123", 
            Role = "SuperAdmin",
            Tenant = masterTenant
        };

        _context.Tenants.Add(masterTenant);
        _context.Users.Add(masterUser);
        await _context.SaveChangesAsync();

        return Ok("Usuario administrador creado. Ya puedes hacer login con admin@detdevs.com / admin123");
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "TU_LLAVE_SUPER_SECRETA_DE_DETDEVS_2024_MINIMO_32_CARACTERES!!";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim("TenantId", user.TenantId.ToString()), 
            new Claim("Role", user.Role ?? "Admin")
        };


        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "rockocloud.api",
            audience: _configuration["Jwt:Audience"] ?? "rockocloud.client",
            claims: claims,
            expires: DateTime.Now.AddDays(7), 
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}