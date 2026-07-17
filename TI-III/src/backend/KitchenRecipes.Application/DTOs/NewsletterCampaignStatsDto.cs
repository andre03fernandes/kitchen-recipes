namespace KitchenRecipes.Application.DTOs;

public sealed record NewsletterCampaignStatsDto(
    int TotalSubscribers,
    int ConfirmedSubscribers,
    int TotalCampaignsSent,
    DateTime? LastCampaignSentAtUtc);
