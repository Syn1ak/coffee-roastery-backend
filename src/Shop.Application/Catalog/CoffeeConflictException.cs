namespace Shop.Application.Catalog;

public class CoffeeConflictException(Guid id) : Exception($"Coffee {id} was changed by someone else.");