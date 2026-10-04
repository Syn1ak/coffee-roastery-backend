using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;
using Shop.Api.Coffee;
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

builder.Services.AddSingleton((sp) =>
{
    RoasterySettings settings = sp.GetRequiredService<IOptions<RoasterySettings>>().Value;
    return NpgsqlDataSource.Create(settings.ConnectionString);
});


builder.Services.AddHealthChecks()       
    .AddCheck<HopperHealthCheck>("hopper"); 

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.UseHttpsRedirection();

app.MapGet("/hopper", (HopperMonitor monitor) => monitor.GetSize());

app.MapGet("/hopper/summary", (HopperMonitor monitor) => monitor.GetSentence());

app.MapGet("/coffees", async (NpgsqlDataSource dataSource) =>
{
   await using var cmd = dataSource.CreateCommand("SELECT id, name, origin, price_per_kg FROM coffees;");
   await using var reader = await cmd.ExecuteReaderAsync();
   var coffees = new List<Coffee>([]);
   while (await reader.ReadAsync())
    {
        coffees.Add(new Coffee(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetDecimal(3)
        ));
    }
   return coffees;
});

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthResponseWriter.WriteJson,
    ResultStatusCodes = new Dictionary<HealthStatus, int>(HealthStatusCodes.ByStatus),
});

app.Run();
