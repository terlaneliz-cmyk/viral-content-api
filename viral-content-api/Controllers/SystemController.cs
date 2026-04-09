using Microsoft.AspNetCore.Mvc;
using ViralContentApi.Data;

namespace ViralContentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public SystemController(
            AppDbContext context,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _context = context;
            _configuration = configuration;
            _environment = environment;
        }

        [HttpGet("health")]
        public async Task<IActionResult> Health()
        {
            var canConnect = await _context.Database.CanConnectAsync();

            if (!canConnect)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    status = "Unhealthy",
                    database = "Unavailable",
                    environment = _environment.EnvironmentName,
                    timestampUtc = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                status = "Healthy",
                database = "Available",
                environment = _environment.EnvironmentName,
                timestampUtc = DateTime.UtcNow
            });
        }

        [HttpGet("info")]
        public IActionResult Info()
        {
            return Ok(new
            {
                name = "viral-content-api",
                version = "1.0",
                environment = _environment.EnvironmentName,
                apiKeyProtectedWriteEndpoints = true,
                publicReadEndpoints = true,
                sqliteEnabled = !string.IsNullOrWhiteSpace(_configuration.GetConnectionString("DefaultConnection")),
                timestampUtc = DateTime.UtcNow
            });
        }

        [HttpGet("boom")]
        public IActionResult Boom()
        {
            throw new Exception("Test exception");
        }
    }
}