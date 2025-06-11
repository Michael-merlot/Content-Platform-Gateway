using Gateway.Api.Middleware;
using Gateway.Core.Interfaces.Clients;
using Gateway.Infrastructure.Clients;
using Gateway.Infrastructure.Logging;
using Gateway.Infrastructure.Monitoring;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Collections.Generic;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, logConfig) =>
    logConfig
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console());

builder.Services.AddControllers();
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
            new string[] { }
        }
    });

    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}");
});

builder.Services.AddHttpClient<ILaravelApiClient, LaravelApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:LaravelApi"] ?? "http://localhost:8000");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IAiServicesClient, AiServicesClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:PythonAiServices"] ?? "http://localhost:5000");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton<MetricsReporter>();

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
