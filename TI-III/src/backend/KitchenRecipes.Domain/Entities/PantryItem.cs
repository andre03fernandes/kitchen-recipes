using KitchenRecipes.Domain.Common;

namespace KitchenRecipes.Domain.Entities;

public sealed class PantryItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal AvailableQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateOnly? ExpirationDate { get; set; }
}
