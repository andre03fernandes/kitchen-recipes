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
                HtmlBody: "<h2>Your Weekly Kitchen Recipes Are Ready</h2><p>Fresh recipes for this week are live in Kitchen Recipes. Explore simple meals, prep faster, and cook with confidence.</p><p><strong>Top picks:</strong> one quick lunch, one family dinner, and one healthy snack.</p><p>Open the app and start cooking today.</p>",
                PlainTextBody: "Your Weekly Kitchen Recipes Are Ready. Fresh recipes for this week are live in Kitchen Recipes. Open the app and start cooking today."),
            new(
                Key: "pantry_rescue",
                Name: "Pantry Rescue",
                Description: "Encourage users to cook with ingredients they already have.",
                Subject: "Turn Pantry Items Into Great Meals",
                HtmlBody: "<h2>Turn Pantry Items Into Great Meals</h2><p>You already have enough ingredients to create excellent recipes.</p><p>Check your pantry list and try a low-waste cooking session today.</p><p>Simple ingredients, smart choices, tasty results.</p>",
                PlainTextBody: "Turn Pantry Items Into Great Meals. Check your pantry list and try a low-waste cooking session today."),
            new(
                Key: "seasonal_special",
                Name: "Seasonal Special",
                Description: "Promote seasonal ingredients and themed cooking ideas.",
                Subject: "Seasonal Flavors Are Here",
                HtmlBody: "<h2>Seasonal Flavors Are Here</h2><p>This season is perfect for colorful, affordable, and nutritious recipes.</p><p>Open Kitchen Recipes to discover curated seasonal meals and tips.</p>",
                PlainTextBody: "Seasonal Flavors Are Here. Open Kitchen Recipes to discover curated seasonal meals and tips."),
            new(
                Key: "healthy_starters",
                Name: "Healthy Starters",
                Description: "Share beginner-friendly healthy meal ideas.",
                Subject: "Healthy Cooking Starts Today",
                HtmlBody: "<h2>Healthy Cooking Starts Today</h2><p>Try balanced recipes with simple steps and clear ingredient lists.</p><p>Perfect if you want healthier habits without complex cooking routines.</p>",
                PlainTextBody: "Healthy Cooking Starts Today. Try balanced recipes with simple steps and clear ingredient lists."),
            new(
                Key: "community_challenge",
                Name: "Community Challenge",
                Description: "Invite subscribers to a shared weekly cooking challenge.",
                Subject: "Join This Week's Cooking Challenge",
                HtmlBody: "<h2>Join This Week's Cooking Challenge</h2><p>Cook one recipe, share your variation, and improve your kitchen skills.</p><p>Challenge theme: fast meals under 30 minutes.</p><p>Let's cook together.</p>",
                PlainTextBody: "Join This Week's Cooking Challenge. Theme: fast meals under 30 minutes. Let's cook together."),
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
