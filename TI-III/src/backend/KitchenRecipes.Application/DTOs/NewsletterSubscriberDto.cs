namespace KitchenRecipes.Application.DTOs;

public sealed record NewsletterSubscriberDto(
    int Id,
    string Email,
    bool IsConfirmed,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record UpsertNewsletterSubscriberRequest(
    string Email,
    bool IsConfirmed);
