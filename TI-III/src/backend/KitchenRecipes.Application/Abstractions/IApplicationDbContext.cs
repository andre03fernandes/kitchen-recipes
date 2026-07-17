using KitchenRecipes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KitchenRecipes.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Recipe> Recipes { get; }
    DbSet<Ingredient> Ingredients { get; }
    DbSet<PantryItem> PantryItems { get; }
    DbSet<AppUser> AppUsers { get; }
    DbSet<AssistantChatThread> AssistantChatThreads { get; }
    DbSet<AssistantChatMessage> AssistantChatMessages { get; }
    DbSet<NewsletterSubscriber> NewsletterSubscribers { get; }
    DbSet<NewsletterCampaignAudit> NewsletterCampaignAudits { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
