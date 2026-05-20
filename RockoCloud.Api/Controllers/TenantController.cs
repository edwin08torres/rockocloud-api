using Microsoft.AspNetCore.Mvc;
using RockoCloud.BusinessLogic.Interfaces;
using RockoCloud.BusinessLogic.Services;

namespace RockoCloud.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantController(ITenantService tenantService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetTenants()
        {
            var tenants = await tenantService.GetTenantsAsync();

            return Ok(tenants);
        }
    }
}
