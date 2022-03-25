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
        public IActionResult Get()
        {
            var client = new HealtCheck();
            var defaultOs = client.GetMetrics();

            string onlyWindows = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                onlyWindows = new SystemDataService().getAvailableRAM();
            else
                onlyWindows = "O sistema operacional não é Windows";

            return Ok(new { defaultOs = defaultOs.Free + "MB", onlyWindows = onlyWindows });
        }
    }
}
