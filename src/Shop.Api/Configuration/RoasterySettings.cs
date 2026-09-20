using System.ComponentModel.DataAnnotations;

namespace Shop.Api.Configuration;

public class RoasterySettings
{

    public const string SectionName = "CoffeeRoastery";

    [Required(ErrorMessage = "ShopDisplayName is required")]
    public string ShopDisplayName { get; init; } = String.Empty;

    [Required(ErrorMessage = "SupportEmailAddress is required")]
    [EmailAddress()]
    public string SupportEmailAddress { get; init; } = String.Empty;
}
