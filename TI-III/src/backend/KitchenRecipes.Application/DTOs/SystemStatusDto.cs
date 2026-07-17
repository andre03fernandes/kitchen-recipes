namespace KitchenRecipes.Application.DTOs;

public sealed record SystemStatusDto(string Service, string Status, DateTime TimestampUtc);
