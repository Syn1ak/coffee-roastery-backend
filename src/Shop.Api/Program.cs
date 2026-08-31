using Shop.Api.HopperMonitor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<HopperMonitor>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/hopper", (HopperMonitor monitor) => monitor.GetSize());

app.MapGet("/hopper/summary", (HopperMonitor monitor) => monitor.GetSentence());

app.Run();


public class AppSettings
{
    public string ShopDisplayName { get; set; } = string.Empty;
}
