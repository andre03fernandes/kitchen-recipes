using System.Data;
using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KitchenRecipes.Infrastructure.Services;

public sealed class NewsletterReportingService : INewsletterReportingService
{
    private readonly ApplicationDbContext _dbContext;

    public NewsletterReportingService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NewsletterCampaignStatsDto> GetCampaignStatsAsync(CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "usp_GetNewsletterCampaignStats";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return new NewsletterCampaignStatsDto(0, 0, 0, null);
        }

        var totalSubscribers = reader.GetInt32(reader.GetOrdinal("TotalSubscribers"));
        var confirmedSubscribers = reader.GetInt32(reader.GetOrdinal("ConfirmedSubscribers"));
        var totalCampaignsSent = reader.GetInt32(reader.GetOrdinal("TotalCampaignsSent"));
        var lastCampaignSentOrdinal = reader.GetOrdinal("LastCampaignSentAtUtc");
        var lastCampaignSentAtUtc = reader.IsDBNull(lastCampaignSentOrdinal)
            ? (DateTime?)null
            : reader.GetDateTime(lastCampaignSentOrdinal);

        return new NewsletterCampaignStatsDto(
            TotalSubscribers: totalSubscribers,
            ConfirmedSubscribers: confirmedSubscribers,
            TotalCampaignsSent: totalCampaignsSent,
            LastCampaignSentAtUtc: lastCampaignSentAtUtc);
    }

    public async Task<IReadOnlyList<NewsletterCampaignHistoryItemDto>> GetCampaignHistoryAsync(
        NewsletterCampaignHistoryQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = query.Limit <= 0 ? 20 : Math.Min(query.Limit, 250);

        var historyQuery = _dbContext.NewsletterCampaignAudits
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.TemplateKey))
        {
            var normalizedTemplateKey = query.TemplateKey.Trim();
            historyQuery = historyQuery.Where(x => x.TemplateKey == normalizedTemplateKey);
        }

        if (query.SentFromUtc.HasValue)
        {
            historyQuery = historyQuery.Where(x => x.SentAtUtc >= query.SentFromUtc.Value);
        }

        if (query.SentToUtc.HasValue)
        {
            historyQuery = historyQuery.Where(x => x.SentAtUtc <= query.SentToUtc.Value);
        }

        return await historyQuery
            .OrderByDescending(x => x.SentAtUtc)
            .Take(normalizedLimit)
            .Select(x => new NewsletterCampaignHistoryItemDto(
                x.Id,
                x.TemplateKey,
                x.RecipientsCount,
                x.SentAtUtc))
            .ToListAsync(cancellationToken);
    }
}
