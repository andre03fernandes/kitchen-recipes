using System.Net;
using System.Net.Http.Json;
using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Domain.Common;
using KitchenRecipes.Domain.Entities;
using KitchenRecipes.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KitchenRecipes.Tests.Integration;

public sealed class ApiIntegrationTests
{
    [Fact]
    public async Task Register_Then_Me_ReturnsAuthenticatedUser()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var registerResponse = await client.PostAsJsonAsync("/api/account/register", new RegisterUserRequest("Api Admin", "apiadmin@test.local", "P@ssw0rd123!"));
        var meResponse = await client.GetAsync("/api/account/me");

        registerResponse.EnsureSuccessStatusCode();
        meResponse.EnsureSuccessStatusCode();

        var me = await meResponse.Content.ReadFromJsonAsync<AuthUserDto>();

        Assert.NotNull(me);
        Assert.Equal("apiadmin@test.local", me.Email);
        Assert.Equal(UserRoles.Admin, me.Role);
    }

    [Fact]
    public async Task RegularUser_CannotCreateRecipe()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        await client.PostAsJsonAsync("/api/account/register", new RegisterUserRequest("Admin User", "admin@test.local", "P@ssw0rd123!"));
        await client.PostAsync("/api/account/logout", content: null);
        await client.PostAsJsonAsync("/api/account/register", new RegisterUserRequest("Regular User", "user@test.local", "P@ssw0rd123!"));

        var createResponse = await client.PostAsJsonAsync("/api/recipes", new UpsertRecipeRequest("Unauthorized Recipe", "Should fail", 15, 2));

        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
    }

    [Fact]
    public async Task Admin_CanCreateRecipe_AndFetchIt()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        await client.PostAsJsonAsync("/api/account/register", new RegisterUserRequest("Admin User", "admin@test.local", "P@ssw0rd123!"));

        var createResponse = await client.PostAsJsonAsync("/api/recipes", new UpsertRecipeRequest("Integration Recipe", "Created via API test", 20, 4));
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<RecipeDto>();
        var getResponse = await client.GetAsync($"/api/recipes/{created!.Id}");
        getResponse.EnsureSuccessStatusCode();
        var fetched = await getResponse.Content.ReadFromJsonAsync<RecipeDto>();

        Assert.NotNull(fetched);
        Assert.Equal("Integration Recipe", fetched.Name);
    }

    [Fact]
    public async Task Admin_CanSendCampaign_ToConfirmedSubscribersOnly()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        await factory.SeedAsync(db =>
        {
            db.NewsletterSubscribers.AddRange(
                new NewsletterSubscriber { Email = "confirmed1@test.local", IsConfirmed = true },
                new NewsletterSubscriber { Email = "confirmed2@test.local", IsConfirmed = true },
                new NewsletterSubscriber { Email = "pending@test.local", IsConfirmed = false });
        });

        await client.PostAsJsonAsync("/api/account/register", new RegisterUserRequest("Admin User", "admin@test.local", "P@ssw0rd123!"));

        var sendResponse = await client.PostAsJsonAsync("/api/newsletter-campaigns/send", new SendNewsletterCampaignRequest("weekly_menu"));
        sendResponse.EnsureSuccessStatusCode();

        var result = await sendResponse.Content.ReadFromJsonAsync<SendNewsletterCampaignResult>();
        Assert.NotNull(result);
        Assert.Equal(2, result.RecipientsCount);
        Assert.Equal(2, factory.NewsletterSender.SentMessages.Count);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(1, await dbContext.NewsletterCampaignAudits.CountAsync());
    }

    [Fact]
    public async Task Admin_CanGetNewsletterStats()
    {
        await using var factory = new CustomWebApplicationFactory(services =>
        {
            services.RemoveAll<INewsletterReportingService>();
            services.AddSingleton<INewsletterReportingService>(new FakeNewsletterReportingService(
                new NewsletterCampaignStatsDto(25, 20, 7, new DateTime(2026, 7, 8, 10, 30, 0, DateTimeKind.Utc))));
        });

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        await client.PostAsJsonAsync("/api/account/register", new RegisterUserRequest("Admin User", "admin@test.local", "P@ssw0rd123!"));

        var response = await client.GetAsync("/api/newsletter-campaigns/stats");
        response.EnsureSuccessStatusCode();

        var stats = await response.Content.ReadFromJsonAsync<NewsletterCampaignStatsDto>();

        Assert.NotNull(stats);
        Assert.Equal(25, stats.TotalSubscribers);
        Assert.Equal(20, stats.ConfirmedSubscribers);
        Assert.Equal(7, stats.TotalCampaignsSent);
        Assert.Equal(new DateTime(2026, 7, 8, 10, 30, 0, DateTimeKind.Utc), stats.LastCampaignSentAtUtc);
    }

    [Fact]
    public async Task RegularUser_CannotGetNewsletterStats()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        await client.PostAsJsonAsync("/api/account/register", new RegisterUserRequest("Admin User", "admin@test.local", "P@ssw0rd123!"));
        await client.PostAsync("/api/account/logout", content: null);
        await client.PostAsJsonAsync("/api/account/register", new RegisterUserRequest("Regular User", "user@test.local", "P@ssw0rd123!"));

        var response = await client.GetAsync("/api/newsletter-campaigns/stats");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanFilterCampaignHistory_ByTemplateKey()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        await factory.SeedAsync(db =>
        {
            db.NewsletterCampaignAudits.AddRange(
                new NewsletterCampaignAudit { TemplateKey = "weekly_menu", RecipientsCount = 10, SentAtUtc = DateTime.UtcNow.AddDays(-1) },
                new NewsletterCampaignAudit { TemplateKey = "weekly_menu", RecipientsCount = 12, SentAtUtc = DateTime.UtcNow.AddDays(-2) },
                new NewsletterCampaignAudit { TemplateKey = "pantry_rescue", RecipientsCount = 8, SentAtUtc = DateTime.UtcNow.AddDays(-3) });
        });

        await client.PostAsJsonAsync("/api/account/register", new RegisterUserRequest("Admin User", "admin@test.local", "P@ssw0rd123!"));

        var response = await client.GetAsync("/api/newsletter-campaigns/history?templateKey=weekly_menu&limit=10");
        response.EnsureSuccessStatusCode();

        var history = await response.Content.ReadFromJsonAsync<List<NewsletterCampaignHistoryItemDto>>();

        Assert.NotNull(history);
        Assert.Equal(2, history.Count);
        Assert.All(history, item => Assert.Equal("weekly_menu", item.TemplateKey));
    }

    [Fact]
    public async Task Admin_CanExportFilteredCampaignHistoryCsv()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        await factory.SeedAsync(db =>
        {
            db.NewsletterCampaignAudits.AddRange(
                new NewsletterCampaignAudit { TemplateKey = "weekly_menu", RecipientsCount = 10, SentAtUtc = DateTime.UtcNow.AddDays(-1) },
                new NewsletterCampaignAudit { TemplateKey = "pantry_rescue", RecipientsCount = 8, SentAtUtc = DateTime.UtcNow.AddDays(-3) });
        });

        await client.PostAsJsonAsync("/api/account/register", new RegisterUserRequest("Admin User", "admin@test.local", "P@ssw0rd123!"));

        var response = await client.GetAsync("/api/newsletter-campaigns/history/export?templateKey=pantry_rescue&limit=10");
        response.EnsureSuccessStatusCode();

        var csv = await response.Content.ReadAsStringAsync();

        Assert.Contains("Id,TemplateKey,RecipientsCount,SentAtUtc", csv);
        Assert.Contains("\"pantry_rescue\"", csv);
        Assert.DoesNotContain("\"weekly_menu\"", csv);
    }

    [Fact]
    public async Task Admin_CanPromoteUser_ThenUserCanAccessAdminRecipeWrite()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        await client.PostAsJsonAsync("/api/account/register", new RegisterUserRequest("Admin User", "admin@test.local", "P@ssw0rd123!"));
        await client.PostAsync("/api/account/logout", content: null);
        await client.PostAsJsonAsync("/api/account/register", new RegisterUserRequest("Regular User", "user@test.local", "P@ssw0rd123!"));

        var meBeforePromotion = await client.GetFromJsonAsync<AuthUserDto>("/api/account/me");
        Assert.NotNull(meBeforePromotion);
        Assert.Equal(UserRoles.User, meBeforePromotion.Role);

        await client.PostAsync("/api/account/logout", content: null);
        await client.PostAsJsonAsync("/api/account/login", new LoginRequest("admin@test.local", "P@ssw0rd123!"));

        var users = await client.GetFromJsonAsync<List<AuthUserDto>>("/api/account/users");
        var regularUser = Assert.Single(users!.Where(user => user.Email == "user@test.local"));

        var updateResponse = await client.PutAsJsonAsync($"/api/account/users/{regularUser.Id}/role", new UpdateUserRoleRequest(UserRoles.Admin));
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        await client.PostAsync("/api/account/logout", content: null);
        await client.PostAsJsonAsync("/api/account/login", new LoginRequest("user@test.local", "P@ssw0rd123!"));

        var createRecipeResponse = await client.PostAsJsonAsync("/api/recipes", new UpsertRecipeRequest("Promoted Admin Recipe", "Created after role promotion", 25, 4));
        createRecipeResponse.EnsureSuccessStatusCode();

        var meAfterPromotion = await client.GetFromJsonAsync<AuthUserDto>("/api/account/me");
        Assert.NotNull(meAfterPromotion);
        Assert.Equal(UserRoles.Admin, meAfterPromotion.Role);
    }

    private sealed class FakeNewsletterReportingService : INewsletterReportingService
    {
        private readonly NewsletterCampaignStatsDto _stats;

        public FakeNewsletterReportingService(NewsletterCampaignStatsDto stats)
        {
            _stats = stats;
        }

        public Task<NewsletterCampaignStatsDto> GetCampaignStatsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_stats);
        }

        public Task<IReadOnlyList<NewsletterCampaignHistoryItemDto>> GetCampaignHistoryAsync(
            NewsletterCampaignHistoryQueryDto query,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<NewsletterCampaignHistoryItemDto>>(Array.Empty<NewsletterCampaignHistoryItemDto>());
        }
    }
}
