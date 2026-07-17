using KitchenRecipes.Application.DTOs;

namespace KitchenRecipes.Application.Abstractions;

public interface INewsletterCampaignService
{
    Task<IReadOnlyList<NewsletterTemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken = default);
    Task<SendNewsletterCampaignResult> SendTemplateAsync(SendNewsletterCampaignRequest request, CancellationToken cancellationToken = default);
}
