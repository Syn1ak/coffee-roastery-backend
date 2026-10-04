namespace Shop.Api.Coffee;

record Coffee(
    int Id,
    string Name,
    string Origin,
    decimal PricePerKg
);