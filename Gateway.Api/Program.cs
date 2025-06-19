using Gateway.Api.Middleware;
using Gateway.Api.Options;
using Gateway.Core.Health;
using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Interfaces.Clients;
using Gateway.Core.Interfaces.History;
using Gateway.Core.Interfaces.Persistence;
using Gateway.Core.Interfaces.Cache;
using Gateway.Core.Middleware;
using Gateway.Core.Monitoring;
using Gateway.Core.Resilience;
using Gateway.Core.Services;
using Gateway.Core.Services.Auth;
using Gateway.Core.Services.History;
using Gateway.Core.Services.Http;
using Gateway.Infrastructure.Extensions;
using Gateway.Infrastructure.Monitoring;
using Gateway.Infrastructure.Persistence.DistributedCache;
using Gateway.Infrastructure.Persistence.tempDB;
using Gateway.Infrastructure.Persistence.Memory;
using Gateway.Infrastructure.Persistence.Redis;
using Gateway.Infrastructure.Persistence.MultiLevel;
using Gateway.Infrastructure.Services.Cache;
using Microsoft.Extensions.Caching.Memory;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using System.Reflection;
using Gateway.Core.Interfaces.Subscriptions;
using Gateway.Core.Services.Subscriptions;
using System.Net.Sockets;
using Gateway.Infrastructure.Clients.Realtime;
using Gateway.Core.Interfaces.Notifications;
using Gateway.Core.Services.Notifications;
using Gateway.Infrastructure.BackgroundServices;
using Gateway.Infrastructure.Persistence.InMemory;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    string localConfigPath = Path.Combine(AppContext.BaseDirectory, "appsettings.Development.local.json");
    if (File.Exists(localConfigPath))
    {
        builder.Configuration.AddJsonFile(localConfigPath, optional: true, reloadOnChange: true);
    }
}
var logger = Log.ForContext<Program>();

builder.Host.UseSerilog((context, logConfig) =>
    logConfig
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console());

// настройка поведения BackgroundService для предотвращения остановки приложения при ошибках
builder.Services.Configure<HostOptions>(options => {
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.AddOptions<AuthOptions>().BindConfiguration("Auth");
builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();
builder.Services.AddScoped<IHistoryService, HistoryService>();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

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
            Array.Empty<string>()
        }
    });

    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}");
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            // В продакшене замените "*" на конкретные домены вашего фронтенда
            builder.WithOrigins("http://localhost:3000", "http://localhost:5000") // Пример: ваш фронтенд
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials(); // Необходимо для SignalR с авторизацией
        });
});

// --- Регистрация сервисов для уведомлений ---
// Репозиторий (для демонстрации в памяти, позже заменится на EF Core)
builder.Services.AddSingleton<INotificationRepository, InMemoryNotificationRepository>();

// Клиент реального времени (SignalR)
builder.Services.AddSingleton<INotificationRealtimeClient, SignalRNotificationRealtimeClient>();

// Сервис уведомлений
builder.Services.AddTransient<INotificationService, NotificationService>();

// SignalR
builder.Services.AddSignalR();

// Фоновая служба для отложенных/периодических уведомлений
builder.Services.AddHostedService<DelayedNotificationHostedService>();
// --- Конец регистрации сервисов для уведомлений ---

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddSingleton<MetricsReporter>();
builder.Services.AddApplicationServices(builder.Configuration);

// проверяем доступность Redis и настраиваем особенности работы приложения в зависимости от результата (либо локальное, либо с докером)
bool useRedis = false;
ConnectionMultiplexer redisConnection = null;

try
{
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ??
        builder.Configuration["Redis:ConnectionString"] ??
        "localhost:6379,abortConnect=false,connectTimeout=2000";

    if (!string.IsNullOrEmpty(redisConnectionString))
    {
        // попытка подключения с коротким таймаутом и опцией abortConnect=false
        var options = ConfigurationOptions.Parse(redisConnectionString);
        options.AbortOnConnectFail = false;
        options.ConnectTimeout = 2000; // 2 секунды таймаут
        options.AsyncTimeout = 2000;
        options.SyncTimeout = 2000;

        redisConnection = ConnectionMultiplexer.Connect(options);
        useRedis = redisConnection.IsConnected;

        if (useRedis)
        {
            logger.Information("Redis доступен и будет использоваться для масштабирования");
        }
        else
        {
            logger.Warning("Не удалось подключиться к Redis. Используются локальные альтернативы");
        }
    }
}
catch (Exception ex)
{
    logger.Warning(ex, "Ошибка подключения к Redis. Используются локальные альтернативы");
}

// общие сервисы, не зависящие от Redis
builder.Services.AddSingleton<IMemoryCacheRepository, MemoryCacheRepository>();
builder.Services.AddSingleton<CircuitBreakerPolicyProvider>();
builder.Services.AddSingleton<ConfigurationSyncService>();
builder.Services.AddSingleton<MetricsService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<MetricsService>());

// конфигурация сервисов в зависимости от доступности Redis
if (useRedis)
{
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ??
        builder.Configuration["Redis:ConnectionString"] ??
        "localhost:6379,abortConnect=false";

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
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
        .PersistKeysToStackExchangeRedis(redisConnection, "DataProtection-Keys");

    builder.Services.AddSingleton<IDistributedCacheService, RedisDistributedCache>();
    builder.Services.AddSingleton<ICacheInvalidator, RedisCacheInvalidator>();
    builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);
    builder.Services.AddHostedService<RedisCacheInvalidationListener>();

    builder.Services.AddHealthChecks()
        .AddCheck<ApiGatewayHealthCheck>("api-gateway", tags: new[] { "ready", "api" })
        .AddRedis(redisConnectionString, name: "redis-cache", tags: new[] { "ready", "cache" });
}
else
{
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.Cookie.Name = "Gateway.Session";
        options.IdleTimeout = TimeSpan.FromHours(1);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

    builder.Services.AddDataProtection();

    builder.Services.AddSingleton<IDistributedCacheService, InMemoryDistributedCache>();
    builder.Services.AddSingleton<ICacheInvalidator, LocalCacheInvalidator>();

    builder.Services.AddHealthChecks()
        .AddCheck<ApiGatewayHealthCheck>("api-gateway", tags: new[] { "ready", "api" });

    logger.Information("API Gateway использует локальный кэш. Горизонтальное масштабирование ограничено.");
}

// многоуровневое кэширование с разными стратегиями в зависимости от доступности Redis
builder.Services.AddSingleton<IMultiLevelCacheRepository, MultiLevelCacheRepository>(sp =>
{
    var memory = sp.GetRequiredService<IMemoryCacheRepository>();
    var distributed = sp.GetRequiredService<IDistributedCacheService>();
    return new MultiLevelCacheRepository(memory, distributed);
});

builder.Services.AddScoped<ISubscriptionService, SubscriptionService>(sp =>
    new SubscriptionService(
        sp.GetRequiredService<IMultiLevelCacheRepository>(),
        sp.GetRequiredService<ICacheInvalidator>()
    )
);
// ConfigurationSyncService в качестве hosted service с защитой от ошибок
builder.Services.AddHostedService(sp => sp.GetRequiredService<ConfigurationSyncService>());

if (!string.IsNullOrEmpty(builder.Configuration["ServiceConfiguration:LaravelApi"]))
{
    try
    {
        builder.Services.AddHealthChecks()
            .AddUrlGroup(
                new Uri(builder.Configuration["ServiceConfiguration:LaravelApi"] + "/health"),
                name: "laravel-api",
                tags: new[] { "ready", "api" });
    }
    catch (Exception ex)
    {
        logger.Warning(ex, "Невозможно настроить health check для LaravelApi");
    }
}

if (!string.IsNullOrEmpty(builder.Configuration["ServiceConfiguration:RecommendationService"]))
{
    try
    {
        builder.Services.AddHealthChecks()
            .AddUrlGroup(
                new Uri(builder.Configuration["ServiceConfiguration:RecommendationService"] + "/health"),
                name: "recommendation-service",
                tags: new[] { "ready", "api" });
    }
    catch (Exception ex)
    {
        logger.Warning(ex, "Невозможно настроить health check для RecommendationService");
    }
}

// Настройка отказоустойчивого HTTP клиента
builder.Services.AddHttpClient<ResilientHttpClient>((sp, client) =>
{
    if (!string.IsNullOrEmpty(builder.Configuration["ServiceConfiguration:LaravelApi"]))
    {
        client.BaseAddress = new Uri(builder.Configuration["ServiceConfiguration:LaravelApi"]);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(30);
    }
})
.AddHttpMessageHandler(() => new TimeoutHandler(TimeSpan.FromSeconds(10)));

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Configuration.GetValue<bool>("EnableMetricsMiddleware", true))
{
    app.UseMiddleware<MetricsMiddleware>();
}

app.UseSerilogRequestLogging();

// Конфигурация health checks
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Конфигурация среды разработки
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
app.UseSession();
app.UseRouting();
// CORS конфигурация
app.UseCors(policy =>
{
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub"); // Маппинг SignalR Hub

app.Run();

public partial class Program { }

/// <summary>
/// Обработчик таймаутов для HTTP запросов
/// </summary>
public class TimeoutHandler : DelegatingHandler
{
    private readonly TimeSpan _timeout;

    public TimeoutHandler(TimeSpan timeout)
    {
        _timeout = timeout;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var cts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

        cts.CancelAfter(_timeout);

        try
        {
            return await base.SendAsync(request, linkedCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Request timed out after {_timeout.TotalSeconds} seconds");
        }
    }
}

/// <summary>
/// Локальная реализация IDistributedCacheService в памяти
/// Класс используется, когда Redis недоступен
/// </summary>
public class InMemoryDistributedCache : IDistributedCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<InMemoryDistributedCache> _logger;
    private readonly Dictionary<string, bool> _keysTracker = new();

    public InMemoryDistributedCache(IMemoryCache memoryCache, ILogger<InMemoryDistributedCache> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _logger.LogInformation("Используется локальный кэш для хранения данных. Масштабирование ограничено.");
    }

    public Task<bool> ExistsAsync(string key)
    {
        return Task.FromResult(_keysTracker.ContainsKey(key));
    }

    public Task<T> GetAsync<T>(string key) where T : class
    {
        if (_memoryCache.TryGetValue(key, out string data) && !string.IsNullOrEmpty(data))
        {
            try
            {
                var result = System.Text.Json.JsonSerializer.Deserialize<T>(data);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка десериализации данных из кэша для ключа {Key}", key);
            }
        }

        return Task.FromResult<T>(null);
    }

    public Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        _keysTracker.Remove(key);
        return Task.CompletedTask;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
            options.SetAbsoluteExpiration(expiration.Value);
        else
            options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

        var data = System.Text.Json.JsonSerializer.Serialize(value);
        _memoryCache.Set(key, data, options);
        _keysTracker[key] = true;

        return Task.CompletedTask;
    }
}

/// <summary>
/// Локальная реализация ICacheInvalidator для работы без Redis
/// </summary>
public class LocalCacheInvalidator : ICacheInvalidator
{
    private readonly ILogger<LocalCacheInvalidator> _logger;

    public LocalCacheInvalidator(ILogger<LocalCacheInvalidator> logger)
    {
        _logger = logger;
    }

    public Task InvalidateCacheAsync(string key)
    {
        _logger.LogInformation("Локальная инвалидация кэша для ключа: {Key}", key);
        return Task.CompletedTask;
    }

    public Task PublishInvalidationAsync(string key)
    {
        _logger.LogInformation("Локальная публикация инвалидации кэша для ключа: {Key}", key);
        return Task.CompletedTask;
    }
}

