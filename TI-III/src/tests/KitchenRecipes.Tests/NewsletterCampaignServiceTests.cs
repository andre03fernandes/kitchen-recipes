using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Application.Services.Newsletter;
using KitchenRecipes.Domain.Entities;
using KitchenRecipes.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace KitchenRecipes.Tests;

public sealed class NewsletterCampaignServiceTests
{
    [Fact]
    public async Task SendTemplateAsync_SendsOnlyToConfirmedSubscribers_AndCreatesAudit()
    {
        await using var context = TestApplicationDbContextFactory.Create();
        context.NewsletterSubscribers.AddRange(
            new NewsletterSubscriber { Email = "confirmed1@test.local", IsConfirmed = true },
            new NewsletterSubscriber { Email = "confirmed2@test.local", IsConfirmed = true },
            new NewsletterSubscriber { Email = "pending@test.local", IsConfirmed = false });
        await context.SaveChangesAsync();

        var sender = new FakeNewsletterSender();
        var service = new NewsletterCampaignService(context, sender);

        var result = await service.SendTemplateAsync(new SendNewsletterCampaignRequest("weekly_menu"));

        Assert.Equal("weekly_menu", result.TemplateKey);
        Assert.Equal(2, result.RecipientsCount);
        Assert.Equal(2, sender.SentMessages.Count);
        Assert.DoesNotContain(sender.SentMessages, x => x.RecipientEmail == "pending@test.local");
        Assert.Equal(1, await context.NewsletterCampaignAudits.CountAsync());
    }

    [Fact]
    public async Task SendTemplateAsync_InvalidTemplate_ThrowsArgumentException()
    {
        await using var context = TestApplicationDbContextFactory.Create();
        var sender = new FakeNewsletterSender();
        var service = new NewsletterCampaignService(context, sender);

        var action = async () => await service.SendTemplateAsync(new SendNewsletterCampaignRequest("invalid_template"));

        var exception = await Assert.ThrowsAsync<ArgumentException>(action);
        Assert.Equal("Template key is invalid.", exception.Message);
    }
}
