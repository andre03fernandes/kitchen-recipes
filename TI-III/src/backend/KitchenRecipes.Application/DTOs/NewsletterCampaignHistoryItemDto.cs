namespace KitchenRecipes.Application.DTOs;

public sealed record NewsletterCampaignHistoryItemDto(
    int Id,
    string TemplateKey,
    int RecipientsCount,
    DateTime SentAtUtc);
