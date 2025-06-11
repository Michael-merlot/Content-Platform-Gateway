using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Gateway.Infrastructure.Logging
{
    public class SerilogConfigurator
    {
        public static void Configure(IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                .WriteTo.File(new JsonFormatter(), "logs/gateway-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)
                .CreateLogger();

            services.AddLogging(builder => builder.AddSerilog(dispose: true));
        }
    }
}
