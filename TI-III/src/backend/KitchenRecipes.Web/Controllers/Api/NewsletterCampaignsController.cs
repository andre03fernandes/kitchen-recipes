using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KitchenRecipes.Web.Controllers.Api;

[ApiController]
[Route("api/newsletter-campaigns")]
[Authorize(Roles = UserRoles.Admin)]
public sealed class NewsletterCampaignsController : ControllerBase
{
    [HttpGet("history/export")]
    public async Task<IActionResult> ExportHistoryCsv(
        [FromServices] INewsletterReportingService newsletterReportingService,
        [FromQuery] string? templateKey,
        [FromQuery] DateTime? sentFromUtc,
        [FromQuery] DateTime? sentToUtc,
        [FromQuery] int limit,
        CancellationToken cancellationToken)
    {
        var history = await newsletterReportingService.GetCampaignHistoryAsync(
            new NewsletterCampaignHistoryQueryDto(templateKey, sentFromUtc, sentToUtc, limit),
            cancellationToken);

        var lines = new List<string>
        {
            "Id,TemplateKey,RecipientsCount,SentAtUtc"
        };

        lines.AddRange(history.Select(item =>
            string.Join(",",
                item.Id,
                EscapeCsvValue(item.TemplateKey),
                item.RecipientsCount,
                item.SentAtUtc.ToString("O"))));

        var csvContent = string.Join(Environment.NewLine, lines);
        var fileName = $"newsletter-campaign-history-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(System.Text.Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromServices] INewsletterReportingService newsletterReportingService,
        [FromQuery] string? templateKey,
        [FromQuery] DateTime? sentFromUtc,
        [FromQuery] DateTime? sentToUtc,
        [FromQuery] int limit,
        CancellationToken cancellationToken)
    {
        var history = await newsletterReportingService.GetCampaignHistoryAsync(
            new NewsletterCampaignHistoryQueryDto(templateKey, sentFromUtc, sentToUtc, limit),
            cancellationToken);
        return Ok(history);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(
        [FromServices] INewsletterReportingService newsletterReportingService,
        CancellationToken cancellationToken)
    {
        var stats = await newsletterReportingService.GetCampaignStatsAsync(cancellationToken);
        return Ok(stats);
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates(
        [FromServices] INewsletterCampaignService newsletterCampaignService,
        CancellationToken cancellationToken)
    {
        var templates = await newsletterCampaignService.GetTemplatesAsync(cancellationToken);
        return Ok(templates);
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send(
        [FromBody] SendNewsletterCampaignRequest request,
        [FromServices] INewsletterCampaignService newsletterCampaignService,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await newsletterCampaignService.SendTemplateAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    private static string EscapeCsvValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "\"\"";
        }

        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}
