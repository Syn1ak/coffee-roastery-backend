using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shop.Api.Configuration;
using Shop.Api.HealthChecks;
using Shop.Api.Hopper;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddOptions<RoasterySettings>()
    .Bind(builder.Configuration.GetSection(RoasterySettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<HopperMonitor>();


builder.Services.AddHealthChecks()       
    .AddCheck<HopperHealthCheck>("hopper"); 

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.UseHttpsRedirection();

app.MapGet("/hopper", (HopperMonitor monitor) => monitor.GetSize());

app.MapGet("/hopper/summary", (HopperMonitor monitor) => monitor.GetSentence());

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthResponseWriter.WriteJson,
    ResultStatusCodes = new Dictionary<HealthStatus, int>(HealthStatusCodes.ByStatus),
});

app.Run();
