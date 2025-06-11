using Gateway.Core.Interfaces.Clients;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Gateway.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILaravelApiClient _laravelApiClient;
        private readonly IAiServicesClient _aiServicesClient;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            ILaravelApiClient laravelApiClient,
            IAiServicesClient aiServicesClient,
            ILogger<HealthController> logger)
        {
            _laravelApiClient = laravelApiClient;
            _aiServicesClient = aiServicesClient;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(HealthResponse), 200)]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation("Health check requested at {Time}", DateTime.UtcNow);

            var laravelApiStatus = await _laravelApiClient.IsHealthyAsync();
            var aiServicesStatus = await _aiServicesClient.IsHealthyAsync();

            var response = new HealthResponse
            {
                Status = "healthy",
                Timestamp = DateTime.UtcNow,
                Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                Dependencies = new Dictionary<string, bool>
                {
                    { "LaravelApi", laravelApiStatus },
                    { "AiServices", aiServicesStatus }
                }
            };

            return Ok(response);
        }

        public class HealthResponse
        {
            public string Status { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
            public string Version { get; set; } = string.Empty;
            public string Environment { get; set; } = string.Empty;
            public Dictionary<string, bool> Dependencies { get; set; } = new();
        }
    }
}
