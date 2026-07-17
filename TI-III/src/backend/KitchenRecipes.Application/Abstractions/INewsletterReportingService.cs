using KitchenRecipes.Application.DTOs;

namespace KitchenRecipes.Application.Abstractions;

public interface INewsletterReportingService
{
    Task<NewsletterCampaignStatsDto> GetCampaignStatsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NewsletterCampaignHistoryItemDto>> GetCampaignHistoryAsync(NewsletterCampaignHistoryQueryDto query, CancellationToken cancellationToken = default);
}
