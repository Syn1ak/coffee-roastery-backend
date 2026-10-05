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

app.MapPost("/coffees", async (CoffeeeCreateDto createDto, NpgsqlDataSource dataSource) =>
{
   await using var cmd = dataSource.CreateCommand($"INSERT INTO coffees (name, origin, price_per_kg, grade) VALUES ($1, $2, $3, $4);");
    cmd.Parameters.Add(new() { Value = createDto.Name });
    cmd.Parameters.Add(new() { Value = createDto.Origin });
    cmd.Parameters.Add(new() { Value = createDto.PricePerKg });
    cmd.Parameters.Add(new() { Value = createDto.Grade });
   var createdCount = await cmd.ExecuteNonQueryAsync();
   return createdCount;
});

app.MapGet("/coffees", async (string? origin, NpgsqlDataSource dataSource) =>
{
    var sql = origin is null
        ? "SELECT id, name, origin, grade, price_per_kg FROM coffees"
        : "SELECT id, name, origin, grade, price_per_kg FROM coffees WHERE origin = $1";
   await using var cmd = dataSource.CreateCommand(sql);
   if (origin is not null)
        cmd.Parameters.Add(new() { Value = origin });
   await using var reader = await cmd.ExecuteReaderAsync();
   var coffees = new List<CoffeeResponseDto>([]);
   while (await reader.ReadAsync())
    {
        coffees.Add(new CoffeeResponseDto(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetDecimal(4)
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
