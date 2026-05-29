using Microsoft.AspNetCore.Mvc;
using RockoCloud.BusinessLogic.Interfaces;
using RockoCloud.BusinessLogic.Services;
using RockoCloud.Models.DTO;

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

        [HttpPost]
        public async Task<IActionResult> SaveTenant([FromBody] TenantSaveDTO tenantSaveDTO)
        {
            try
            {
                var tenant = await tenantService.SaveTenantAsync(tenantSaveDTO);

                return Ok(tenant);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}