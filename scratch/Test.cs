using System;

var ticker = new StockTicker("Apple");
var emailAlertObserver = new EmailAlertObserver();

ticker.PriceChanged += emailAlertObserver.OnPriceChanged;

ticker.UpdatePrice(150.25m);


(int, string) tup = (1, "sad");

int num = tup.Item1;
string num2 = tup.Item2;

var (valu1, str1) = tup;

(int Min, int Max) FindMinMax(int[] numbers)
{
    return (numbers.Min(), numbers.Max());
}

string nul = null;
nul ??= "Sas";


Console.WriteLine($"null: {nul}");

var result = FindMinMax(new [] {1, 2, 3, 4, 5, 6});
Console.WriteLine($"min: {result.Min}, max: {result.Max}");

Repository<int> rep = new();
rep.Add(1);
rep.Add(2);

Console.WriteLine($"el: {rep.Get(0)}");
rep.PrintAll();

Console.WriteLine("Before touching Person at all");

int[] numbers = [1, 2, 3, 4, 5];

int[] more = [0, .. numbers, 6];


async Task<string> DownloadDataAsync(string url)
{
    // Create HTTP client
    using HttpClient client = new HttpClient();
    
    // Asynchronously wait for the HTTP request
    string result = await client.GetStringAsync(url);
    
    return result;
}


async Task ProcessMultipleAsync()
{
    Task<string> task1 = DownloadDataAsync("https://jsonplaceholder.typicode.com/posts/1");
    Task<string> task2 = DownloadDataAsync("https://jsonplaceholder.typicode.com/posts/2");
    Task<string> task3 = DownloadDataAsync("https://jsonplaceholder.typicode.com/posts/3");
    
    // Wait for all tasks to complete
    string[] results = await Task.WhenAll(task1, task2, task3);
    
    // Process results
    foreach (string result in results)
    {
        Console.WriteLine($"Result length: {result.Length}");
    }
}

await ProcessMultipleAsync();

public class Person
{
    public string Name { set; get; }
    public int Age { set; get; }

    public Person()
    {
        Name = "Nothing";
        Age = 0;
    }

    static Person()
    {
        Console.WriteLine("Hello");
    }
}


public class Repository<T>
{
    private readonly List<T> _list = new List<T>();

    public void Add(T item) => _list.Add(item);
    public T Get(int index) => _list[index];
    
    public void PrintAll() => Console.WriteLine(string.Join(", ", _list));
}

public class StockTicker
{
    public delegate void PriceChangeHandler(string symbol, decimal price);

    public event PriceChangeHandler? PriceChanged;

    private decimal _price;

    public string Symbol { get; }

    public StockTicker(string symbol) => Symbol = symbol;

    public void UpdatePrice(decimal newPrice)
    {
        _price = newPrice;
        PriceChanged?.Invoke(Symbol, newPrice);
    }
}

public class EmailAlertObserver
{
    public void OnPriceChanged(string symbol, decimal newPrice)
    {
        Console.WriteLine($"{symbol} changed to {newPrice}");
    }
}