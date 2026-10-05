namespace Shop.Api.Coffee;

record CoffeeResponseDto(
    int Id,
    string Name,
    string Origin,
    string Grade,
    decimal PricePerKg
);

record CoffeeeCreateDto(
    string Name,
    string Origin,
    string Grade,
    decimal PricePerKg
);

