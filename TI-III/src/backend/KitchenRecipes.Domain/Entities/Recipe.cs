using KitchenRecipes.Domain.Common;

namespace KitchenRecipes.Domain.Entities;

public sealed class Recipe : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int PreparationMinutes { get; set; }
    public int Servings { get; set; }
    public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
}
