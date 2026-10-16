using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;
using Shop.Api.Coffee;
using Shop.Api.Configuration;
using Shop.Api.HealthChecks;
using Shop.Api.Hopper;
using Shop.Application;
using Shop.Application.Catalog;
using Shop.Application.Catalog.PublishCoffee;
using Shop.Domain;
using Shop.Infrastructure.Persistence;

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

builder.Services.AddScoped<ICoffeeRepository, CoffeeRepository>(); 

builder.Services.AddScoped<PublishCoffeeHandler>();

builder.Services.AddScoped<NpgsqlUnitOfWork>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<NpgsqlUnitOfWork>());


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

var catalog = app.MapGroup("/catalog");

catalog.MapGet("/published-coffees", async (ICoffeeRepository coffeeRepository, CancellationToken ct) =>
{
    return await coffeeRepository.ListPublishedAsync(ct);
});

catalog.MapPost("/coffees/{id:guid}/publish", async Task<Results<NoContent, NotFound, UnprocessableEntity<string>, Conflict>> (Guid id, PublishCoffeeHandler handler, CancellationToken ct) =>
{
    try
    {
        bool found = await handler.HandleAsync(id, ct);
        return found ? TypedResults.NoContent() : TypedResults.NotFound();
    }
    catch (DomainException e)
    {
        return TypedResults.UnprocessableEntity(e.Message);
    }
    catch (CoffeeConflictException)
    {
        return TypedResults.Conflict();
    }
});

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthResponseWriter.WriteJson,
    ResultStatusCodes = new Dictionary<HealthStatus, int>(HealthStatusCodes.ByStatus),
});

app.Run();
