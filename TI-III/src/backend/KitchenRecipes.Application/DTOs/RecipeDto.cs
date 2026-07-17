namespace KitchenRecipes.Application.DTOs;

public sealed record RecipeDto(
    int Id,
    string Name,
    string Description,
    string? ImageUrl,
    int PreparationMinutes,
    int Servings,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record UpsertRecipeRequest(
    string Name,
    string Description,
    string? ImageUrl,
    int PreparationMinutes,
    int Servings);
