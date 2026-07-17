namespace KitchenRecipes.Application.DTOs;

public sealed record NewsletterCampaignHistoryQueryDto(
    string? TemplateKey,
    DateTime? SentFromUtc,
    DateTime? SentToUtc,
    int Limit = 20);
