using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway.Core.Health
{

    public class ApiGatewayHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {

                return Task.FromResult(HealthCheckResult.Healthy("API Gateway is healthy"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    new HealthCheckResult(
                        context.Registration.FailureStatus,
                        "API Gateway is unhealthy",
                        ex));
            }
        }
    }
}
