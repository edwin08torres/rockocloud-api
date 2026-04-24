using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RockoCloud.DataAccess;
using RockoCloud.Models;
using System.Security.Claims;

namespace RockoCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BranchesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest request)
    {
        var tenantId = Guid.Parse(User.FindFirstValue("TenantId")!);

        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Address = request.Address ?? ""
        };

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();

        return Ok(branch);
    }
}

public record CreateBranchRequest(string Name, string? Address);