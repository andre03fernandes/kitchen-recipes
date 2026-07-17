using KitchenRecipes.Domain.Common;

namespace KitchenRecipes.Domain.Entities;

public sealed class Ingredient : BaseEntity
{
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = default!;
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
}
