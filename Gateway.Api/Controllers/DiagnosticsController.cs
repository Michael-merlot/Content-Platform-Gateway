using Gateway.Core.Interfaces.Persistence;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gateway.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly IDistributedCacheService _cacheService;
        private readonly ILogger<DiagnosticsController> _logger;

        public DiagnosticsController(
            IDistributedCacheService cacheService,
            ILogger<DiagnosticsController> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        [HttpGet("redis-test")]
        public async Task<IActionResult> TestRedis()
        {
            try
            {
                string testKey = "test:key:" + Guid.NewGuid().ToString();
                string testValue = "Redis test at " + DateTime.Now.ToString();

                await _cacheService.SetAsync(testKey, testValue, TimeSpan.FromMinutes(5));
                _logger.LogInformation("Successfully wrote to Redis");

                bool exists = await _cacheService.ExistsAsync(testKey);
                var retrievedValue = await _cacheService.GetAsync<string>(testKey);

                await _cacheService.RemoveAsync(testKey);

                return Ok(new
                {
                    Success = true,
                    KeyExists = exists,
                    OriginalValue = testValue,
                    RetrievedValue = retrievedValue,
                    UsingRedis = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis test failed");
                return StatusCode(500, new
                {
                    Success = false,
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("session-test")]
        public IActionResult TestSession(string value = null)
        {
            if (!string.IsNullOrEmpty(value))
            {
                HttpContext.Session.SetString("TestSessionValue", value);
                return Ok(new
                {
                    Message = $"Session value set to '{value}'",
                    SessionId = HttpContext.Session.Id
                });
            }
            else
            {
                var sessionValue = HttpContext.Session.GetString("TestSessionValue");
                return Ok(new
                {
                    SessionValue = sessionValue ?? "(no value)",
                    SessionId = HttpContext.Session.Id
                });
            }
        }


        [HttpGet("data-protection-test")]
        public async Task<IActionResult> TestDataProtection(string value = "test-data")
        {
            try
            {
                string protectionKey = "protected:data:" + Guid.NewGuid().ToString();

                await _cacheService.SetAsync(protectionKey, value, TimeSpan.FromMinutes(30));

                var retrievedValue = await _cacheService.GetAsync<string>(protectionKey);

                return Ok(new
                {
                    Success = true,
                    OriginalValue = value,
                    RetrievedValue = retrievedValue,
                    KeysLocation = "Using Redis for Data Protection"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Data protection test failed");
                return StatusCode(500, new
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }
    }
}
