using Gateway.Core.Monitoring;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Gateway.Core.Middleware
{
    public class MetricsMiddleware
    {
        private readonly RequestDelegate _next;

        public MetricsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;
            var method = context.Request.Method;

            if (path == "/metrics")
            {
                await _next(context);
                return;
            }

            MetricsService.ActiveConnections.Inc();

            MetricsService.TotalRequests
                .WithLabels(method, path)
                .Inc();

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);

                stopwatch.Stop();
                MetricsService.RequestDuration
                    .WithLabels(method, path, context.Response.StatusCode.ToString())
                    .Observe(stopwatch.Elapsed.TotalSeconds);
            }
            catch (Exception)
            {
                MetricsService.FailedRequests
                    .WithLabels(method, path)
                    .Inc();

                throw;
            }
            finally
            {
                MetricsService.ActiveConnections.Dec();
            }
        }
    }
}
