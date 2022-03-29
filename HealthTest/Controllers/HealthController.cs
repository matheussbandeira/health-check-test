using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace HealthTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        public HealthController()
        {
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var client = new HealtCheck();
            var defaultOs = await client.GetMetrics();

            return Ok(defaultOs);
        }
    }
}
