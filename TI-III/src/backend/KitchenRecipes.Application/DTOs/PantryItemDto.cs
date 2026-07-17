namespace KitchenRecipes.Application.DTOs;

public sealed record PantryItemDto(
    int Id,
    string Name,
    string? ImageUrl,
    decimal AvailableQuantity,
    string Unit,
    DateOnly? ExpirationDate,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record UpsertPantryItemRequest(
    string Name,
    string? ImageUrl,
    decimal AvailableQuantity,
    string Unit,
    DateOnly? ExpirationDate);
