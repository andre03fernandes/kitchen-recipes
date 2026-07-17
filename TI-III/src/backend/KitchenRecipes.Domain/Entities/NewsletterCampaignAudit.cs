using KitchenRecipes.Domain.Common;

namespace KitchenRecipes.Domain.Entities;

public sealed class NewsletterCampaignAudit : BaseEntity
{
    public string TemplateKey { get; set; } = string.Empty;
    public int RecipientsCount { get; set; }
    public DateTime SentAtUtc { get; set; }
}
