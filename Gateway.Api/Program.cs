using Gateway.Api.Middleware;
using Gateway.Api.Options;
using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Interfaces.Clients;
using Gateway.Core.Services.Auth;
using Gateway.Core.Interfaces.History;
using Gateway.Core.Services.History;
using Gateway.Infrastructure.Clients;
using Gateway.Infrastructure.Monitoring;
using Gateway.Infrastructure.Persistence.tempDB;
using Gateway.Infrastructure.Extensions;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using Gateway.Core.Interfaces.Persistence;
using Gateway.Core.Services;
using Gateway.Infrastructure.Persistence.DistributedCache;
using Gateway.Core.Health;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Serilog;

using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, logConfig) =>
    logConfig
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console());

builder.Services.AddOptions<AuthOptions>().BindConfiguration("Auth");

builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();
builder.Services.AddScoped<IHistoryService, HistoryService>();

builder.Services.AddControllers();

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<AuthOptions>>((options, authOptions) =>
    {
        if (builder.Environment.IsDevelopment())
            options.RequireHttpsMetadata = false;

        options.Authority = authOptions.Value.Authority;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    })
    .Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Content Platform Gateway API",
        Version = "v1",
        Description = "API Gateway для обеспечения доступа к сервисам контент-платформы",
        Contact = new OpenApiContact
        {
            Name = "Команда разработки",
            Email = "yandex@mail.ru"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.UseAllOfForInheritance();
    c.UseOneOfForPolymorphism();

    c.SelectSubTypesUsing(baseType =>
        typeof(Program).Assembly.GetTypes().Where(type => type.IsSubclassOf(baseType)));

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT авторизация. Введите 'Bearer' [пробел] и ваш токен.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });

    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}");
});



builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddSingleton<MetricsReporter>();
builder.Services.AddScoped<Gateway.Core.Interfaces.Subscriptions.ISubscriptionService, Gateway.Core.Services.Subscriptions.SubscriptionService>();
builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddScoped<Gateway.Core.Interfaces.Subscriptions.ISubscriptionService, Gateway.Core.Services.Subscriptions.SubscriptionService>();

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "Gateway_";
});

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "Gateway.Session";
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(
        ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")),
        "DataProtection-Keys");

builder.Services.AddScoped<IDistributedCacheService, RedisDistributedCache>();

builder.Services.AddHostedService<ConfigurationSyncService>();

builder.Services.AddHealthChecks()
    .AddRedis(
        builder.Configuration.GetConnectionString("Redis"),
        name: "redis-cache",
        tags: new[] { "ready", "cache" })
    .AddCheck<ApiGatewayHealthCheck>("api-gateway", tags: new[] { "ready", "api" });

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Content Platform Gateway API V1");
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();

app.UseCors(policy =>
{
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
