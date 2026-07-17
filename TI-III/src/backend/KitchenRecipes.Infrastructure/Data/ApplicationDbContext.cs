using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KitchenRecipes.Infrastructure.Data;

public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<PantryItem> PantryItems => Set<PantryItem>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AssistantChatThread> AssistantChatThreads => Set<AssistantChatThread>();
    public DbSet<AssistantChatMessage> AssistantChatMessages => Set<AssistantChatMessage>();
    public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();
    public DbSet<NewsletterCampaignAudit> NewsletterCampaignAudits => Set<NewsletterCampaignAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.ImageUrl);
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 2);
            entity.Property(x => x.Unit).HasMaxLength(24).IsRequired();
            entity.HasOne(x => x.Recipe)
                .WithMany(x => x.Ingredients)
                .HasForeignKey(x => x.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PantryItem>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ImageUrl);
            entity.Property(x => x.AvailableQuantity).HasPrecision(18, 2);
            entity.Property(x => x.Unit).HasMaxLength(24).IsRequired();
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable(table => table.HasTrigger("trg_AppUsers_RoleChange_Audit"));
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<AssistantChatThread>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(160).IsRequired();
            entity.Property(x => x.LastMessagePreview).HasMaxLength(240).IsRequired();
            entity.HasOne(x => x.AppUser)
                .WithMany(x => x.AssistantChatThreads)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.AppUserId, x.UpdatedAtUtc });
            entity.HasIndex(x => x.CreatedAtUtc);
        });

        modelBuilder.Entity<AssistantChatMessage>(entity =>
        {
            entity.Property(x => x.Role).HasMaxLength(24).IsRequired();
            entity.Property(x => x.Content).HasMaxLength(8000).IsRequired();
            entity.Property(x => x.PantryHighlightsJson).HasMaxLength(8000);
            entity.Property(x => x.SuggestedRecipesJson).HasMaxLength(16000);
            entity.Property(x => x.FollowUpPromptsJson).HasMaxLength(8000);
            entity.HasOne(x => x.AssistantChatThread)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.AssistantChatThreadId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.AssistantChatThreadId, x.CreatedAtUtc });
        });

        modelBuilder.Entity<NewsletterSubscriber>(entity =>
        {
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<NewsletterCampaignAudit>(entity =>
        {
            entity.Property(x => x.TemplateKey).HasMaxLength(100).IsRequired();
            entity.Property(x => x.SentAtUtc).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
