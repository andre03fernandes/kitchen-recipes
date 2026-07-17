using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KitchenRecipes.Application.Services.Newsletter;

public sealed class NewsletterCampaignService : INewsletterCampaignService
{
    private readonly IApplicationDbContext _context;
    private readonly INewsletterSender _newsletterSender;

    public NewsletterCampaignService(
        IApplicationDbContext context,
        INewsletterSender newsletterSender)
    {
        _context = context;
        _newsletterSender = newsletterSender;
    }

    public Task<IReadOnlyList<NewsletterTemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<NewsletterTemplateDto> templates =
        [
            new(
                Key: "weekly_menu",
                Name: "Weekly Menu Boost",
                Description: "Highlight a weekly set of practical recipes for subscribers.",
                Subject: "Your Weekly Kitchen Recipes Are Ready",
                                HtmlBody: """
                                                    <div style="margin:0;padding:28px 0;background:#0f1218;font-family:Arial,'Segoe UI',sans-serif;color:#e7eef8;">
                                                        <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%">
                                                            <tr>
                                                                <td align="center">
                                                                    <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="640" style="max-width:640px;background:#1b2431;border:1px solid #4b6487;border-radius:20px;overflow:hidden;">
                                                                        <tr>
                                                                            <td style="background:linear-gradient(135deg,#263243,#1b2431);padding:28px 34px;color:#e7eef8;">
                                                                                <p style="margin:0;font-size:12px;letter-spacing:2px;text-transform:uppercase;opacity:0.9;color:#87b6ff;">Kitchen Recipes</p>
                                                                                <h1 style="margin:10px 0 0;font-size:28px;line-height:1.2;">Your Weekly Menu Is Ready</h1>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td style="padding:30px 34px;">
                                                                                <p style="margin:0 0 16px;font-size:16px;line-height:1.6;">Plan less, cook more. This week brings a practical mix of quick meals and comforting classics.</p>
                                                                                <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="margin:8px 0 18px;">
                                                                                    <tr>
                                                                                        <td style="padding:12px;border:1px solid #5b8fe0;border-radius:12px;background:#263243;font-size:14px;line-height:1.5;color:#c0cfe3;">
                                                                                            <strong>Top picks:</strong> one 15-minute lunch, one family dinner, and one smart snack prep.
                                                                                        </td>
                                                                                    </tr>
                                                                                </table>
                                                                                <a href="https://kitchenrecipes.local/recipes" style="display:inline-block;padding:12px 20px;background:#5b8fe0;color:#0f1218;text-decoration:none;border-radius:10px;font-weight:700;">Open Weekly Recipes</a>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                    """,
                                PlainTextBody: "Weekly Menu Boost: this week includes a 15-minute lunch, a family dinner, and a snack prep plan. Open Kitchen Recipes to see your weekly picks."),
            new(
                Key: "pantry_rescue",
                Name: "Pantry Rescue",
                Description: "Encourage users to cook with ingredients they already have.",
                Subject: "Turn Pantry Items Into Great Meals",
                                HtmlBody: """
                                                    <div style="margin:0;padding:26px 0;background:#0f1218;font-family:Arial,'Segoe UI',sans-serif;color:#e7eef8;">
                                                        <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%">
                                                            <tr>
                                                                <td align="center">
                                                                    <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="620" style="max-width:620px;background:#1b2431;border:1px solid #4b6487;border-radius:18px;overflow:hidden;">
                                                                        <tr>
                                                                            <td style="padding:24px 30px;background:#263243;border-bottom:1px solid #4b6487;">
                                                                                <h2 style="margin:0;font-size:25px;line-height:1.3;color:#87b6ff;">Pantry Rescue Mode</h2>
                                                                                <p style="margin:8px 0 0;font-size:14px;color:#c0cfe3;">Waste less, cook smarter, save money.</p>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td style="padding:26px 30px;">
                                                                                <p style="margin:0 0 14px;font-size:16px;line-height:1.6;">You already have enough ingredients for a complete meal. Start from your pantry and let the assistant suggest combinations.</p>
                                                                                <ul style="margin:0 0 18px;padding-left:18px;font-size:14px;line-height:1.7;color:#c0cfe3;">
                                                                                    <li>Build meals from what expires first</li>
                                                                                    <li>Prioritize low-waste ingredient swaps</li>
                                                                                    <li>Finish leftovers with practical pairings</li>
                                                                                </ul>
                                                                                <a href="https://kitchenrecipes.local/pantry" style="display:inline-block;padding:11px 18px;background:#87b6ff;color:#0f1218;text-decoration:none;border-radius:10px;font-weight:700;">Open Pantry Assistant</a>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                    """,
                                PlainTextBody: "Pantry Rescue: cook with ingredients you already have, reduce waste, and use smart substitutions from your pantry assistant."),
            new(
                Key: "seasonal_special",
                Name: "Seasonal Special",
                Description: "Promote seasonal ingredients and themed cooking ideas.",
                Subject: "Seasonal Flavors Are Here",
                                HtmlBody: """
                                                    <div style="margin:0;padding:32px 0;background:#0f1218;font-family:Georgia,'Times New Roman',serif;color:#e7eef8;">
                                                        <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%">
                                                            <tr>
                                                                <td align="center">
                                                                    <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="640" style="max-width:640px;background:#1b2431;border:1px solid #4b6487;border-radius:0;">
                                                                        <tr>
                                                                            <td style="padding:30px 34px;text-align:center;border-bottom:3px solid #6f6bff;">
                                                                                <p style="margin:0;font-size:12px;letter-spacing:3px;text-transform:uppercase;color:#87b6ff;">Seasonal Collection</p>
                                                                                <h1 style="margin:12px 0 0;font-size:34px;line-height:1.2;color:#e7eef8;">Seasonal Flavors Are Here</h1>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td style="padding:28px 34px;">
                                                                                <p style="margin:0 0 14px;font-size:17px;line-height:1.7;">Fresh produce is at its peak. Discover curated recipes built around seasonal ingredients and balanced nutrition.</p>
                                                                                <p style="margin:0 0 18px;font-size:15px;line-height:1.7;color:#c0cfe3;">This edition highlights colorful bowls, roasted trays, and warm comfort options for the week.</p>
                                                                                <div style="padding:12px 14px;border-left:4px solid #6f6bff;background:#263243;font-size:14px;line-height:1.6;color:#e7eef8;">Chef tip: choose 3 seasonal ingredients and build every meal around them for simpler planning.</div>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                    """,
                                PlainTextBody: "Seasonal Special: discover curated recipes with seasonal ingredients, warm comfort dishes, and practical chef tips for weekly planning."),
            new(
                Key: "healthy_starters",
                Name: "Healthy Starters",
                Description: "Share beginner-friendly healthy meal ideas.",
                Subject: "Healthy Cooking Starts Today",
                                HtmlBody: """
                                                    <div style="margin:0;padding:28px 0;background:#0f1218;font-family:Arial,'Segoe UI',sans-serif;color:#e7eef8;">
                                                        <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%">
                                                            <tr>
                                                                <td align="center">
                                                                    <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="620" style="max-width:620px;background:#1b2431;border:1px solid #4b6487;border-radius:14px;overflow:hidden;">
                                                                        <tr>
                                                                            <td style="padding:24px 28px;background:#263243;color:#e7eef8;">
                                                                                <h2 style="margin:0;font-size:26px;line-height:1.3;">Healthy Cooking Starts Today</h2>
                                                                                <p style="margin:8px 0 0;font-size:14px;color:#c0cfe3;">Simple steps. Real ingredients. Better routines.</p>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td style="padding:26px 28px;">
                                                                                <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="margin-bottom:14px;">
                                                                                    <tr>
                                                                                        <td style="width:50%;padding:10px;border:1px solid #4b6487;background:#263243;font-size:13px;color:#e7eef8;"><strong>Breakfast:</strong> protein + fruit</td>
                                                                                        <td style="width:50%;padding:10px;border:1px solid #4b6487;background:#263243;font-size:13px;color:#e7eef8;"><strong>Lunch:</strong> whole grains + greens</td>
                                                                                    </tr>
                                                                                </table>
                                                                                <p style="margin:0 0 16px;font-size:15px;line-height:1.7;">Get beginner-friendly recipes with clear ingredient lists and predictable prep times.</p>
                                                                                <a href="https://kitchenrecipes.local/recipes" style="display:inline-block;padding:11px 18px;background:#5b8fe0;color:#0f1218;text-decoration:none;border-radius:8px;font-weight:700;">View Healthy Starters</a>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                    """,
                                PlainTextBody: "Healthy Starters: beginner-friendly meal ideas with simple steps, clear ingredients, and better daily routines."),
            new(
                Key: "community_challenge",
                Name: "Community Challenge",
                Description: "Invite subscribers to a shared weekly cooking challenge.",
                Subject: "Join This Week's Cooking Challenge",
                                HtmlBody: """
                                                    <div style="margin:0;padding:24px 0;background:#0f1218;font-family:Arial,'Segoe UI',sans-serif;color:#e7eef8;">
                                                        <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%">
                                                            <tr>
                                                                <td align="center">
                                                                    <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="620" style="max-width:620px;background:#1b2431;border:1px solid #4b6487;border-radius:18px;overflow:hidden;">
                                                                        <tr>
                                                                            <td style="padding:24px 28px;background:linear-gradient(135deg,#6f6bff,#5b8fe0);color:#e7eef8;">
                                                                                <p style="margin:0;font-size:12px;letter-spacing:2px;text-transform:uppercase;color:#e7eef8;">Community Week</p>
                                                                                <h2 style="margin:10px 0 0;font-size:29px;line-height:1.2;">30-Minute Cooking Challenge</h2>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td style="padding:24px 28px;">
                                                                                <p style="margin:0 0 14px;font-size:16px;line-height:1.7;color:#c0cfe3;">Cook one fast recipe, add your personal twist, and compare results with the community.</p>
                                                                                <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="margin:0 0 16px;">
                                                                                    <tr>
                                                                                        <td style="padding:12px;border:1px solid #6f6bff;background:#263243;border-radius:10px;font-size:14px;color:#e7eef8;">
                                                                                            <strong>This week's theme:</strong> complete meals under 30 minutes.
                                                                                        </td>
                                                                                    </tr>
                                                                                </table>
                                                                                <a href="https://kitchenrecipes.local/assistant" style="display:inline-block;padding:11px 18px;background:#6f6bff;color:#e7eef8;text-decoration:none;border-radius:10px;font-weight:700;">Start the Challenge</a>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                    """,
                                PlainTextBody: "Community Challenge: create a complete meal in under 30 minutes, share your variation, and improve your skills together."),
        ];

        return Task.FromResult(templates);
    }

    public async Task<SendNewsletterCampaignResult> SendTemplateAsync(
        SendNewsletterCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.TemplateKey))
        {
            throw new ArgumentException("Template key is required.");
        }

        var templates = await GetTemplatesAsync(cancellationToken);
        var template = templates.FirstOrDefault(x =>
            string.Equals(x.Key, request.TemplateKey.Trim(), StringComparison.OrdinalIgnoreCase));

        if (template is null)
        {
            throw new ArgumentException("Template key is invalid.");
        }

        var recipients = await _context.NewsletterSubscribers
            .AsNoTracking()
            .Where(x => x.IsConfirmed)
            .Select(x => x.Email)
            .ToListAsync(cancellationToken);

        foreach (var recipient in recipients)
        {
            await _newsletterSender.SendAsync(recipient, template.Subject, template.HtmlBody, template.PlainTextBody, cancellationToken);
        }

        var sentAtUtc = DateTime.UtcNow;

        _context.NewsletterCampaignAudits.Add(new NewsletterCampaignAudit
        {
            TemplateKey = template.Key,
            RecipientsCount = recipients.Count,
            SentAtUtc = sentAtUtc,
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new SendNewsletterCampaignResult(
            TemplateKey: template.Key,
            RecipientsCount: recipients.Count,
            SentAtUtc: sentAtUtc);
    }
}
