using Shop.Api.HopperAlertTracker;
using Shop.Api.HopperMonitor;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddScoped<IIdScoped, HopperMonitor>();

builder.Services.AddTransient<IIdTransient, HopperMonitor>();

builder.Services.AddSingleton<IIdSingleton, HopperMonitor>();

builder.Services.AddSingleton<HopperAlertTracker>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/hopper", (IIdSingleton monitor) => monitor.GetSize());

app.MapGet("/hopper/summary", (IIdSingleton monitor) => monitor.GetSentence());

app.MapGet("/hopper/ids", (
    IIdSingleton singleton,
    IIdScoped scopedA, IIdScoped scopedB,
    IIdTransient transientA, IIdTransient transientB) =>
    string.Join('\n',
        $"singleton : {singleton.Id}",
        $"scoped   A: {scopedA.Id}",
        $"scoped   B: {scopedB.Id}",
        $"transient A: {transientA.Id}",
        $"transient B: {transientB.Id}"));

app.MapGet("/hopper/tracker", (HopperAlertTracker tracker, IIdScoped monitor) 
    => string.Join('\n',
        $"tracker id : {tracker.GetStoredId()}",
        $"scoped: {monitor.Id}"));

app.Run();


public class AppSettings
{
    public string ShopDisplayName { get; set; } = string.Empty;
}
