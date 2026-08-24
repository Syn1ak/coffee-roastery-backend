using System.Text;

/// <summary>
/// Adds two integers and returns the result
/// </summary>
/// <param name="a">First integer</param>
/// <param name="b">Second integer</param>
/// <returns>The sum of the two integers</returns>
/// <exception cref="OverflowException">Thrown when the sum is too large</exception>
/// 
int Add(int a, int b) => a + b;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();


    string h = "one";
    string l = "ONE";
    bool length = String.Equals(h, l);
    string summary = h + " " + l;
    string both = $"{h}, l";

    string verbose = $@" dsfsdfsdf
        sdfsdf/'\\\l;{h}
    ";

    string rawInterpolated = $"""
        nice {h}
    """;

    StringBuilder sb = new StringBuilder();
    for(int i = 0; i < 0; i++)
    {
        sb.Append($"Item {i}");
    }
    string result = sb.ToString();

    int age = 21;
    string message = age >= 18 ? "Adult" : "Young";

    DateTime today = DateTime.Now;
    DayOfWeek day = today.DayOfWeek;

    string getDayType(DayOfWeek day) => day switch
    {
        DayOfWeek.Friday => "Nice",
        _ => "Crazy"
    };

    string text = "Hello".Truncate(2);

    Func<int, int, int> add = (a, b) => a + b;
    int sum = add(1, 2);

    Action<string> sayHello = (message) => Console.WriteLine(message);
    Predicate<int> isEven = (num) => num % 2 == 0; 


    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}