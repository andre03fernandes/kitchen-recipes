namespace KitchenRecipes.Application.DTOs;

public sealed record NewsletterTemplateDto(
    string Key,
    string Name,
    string Description,
    string Subject,
    string HtmlBody,
    string PlainTextBody);

public sealed record SendNewsletterCampaignRequest(string TemplateKey);

public sealed record SendNewsletterCampaignResult(
    string TemplateKey,
    int RecipientsCount,
    DateTime SentAtUtc);
