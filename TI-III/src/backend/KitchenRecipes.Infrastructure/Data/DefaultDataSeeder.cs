using KitchenRecipes.Domain.Common;
using KitchenRecipes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace KitchenRecipes.Infrastructure.Data;

public static class DefaultDataSeeder
{
    private const int MinRowsPerTable = 10;
    private const string DefaultPassword = "P@ssw0rd123!";
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100000;

    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        await SeedUsersAsync(context, cancellationToken);
        await SeedSubscribersAsync(context, cancellationToken);
        await SeedPantryItemsAsync(context, cancellationToken);
        await SeedRecipesAndIngredientsAsync(context, cancellationToken);
        await SeedCampaignAuditsAsync(context, cancellationToken);
        await SeedRoleAuditsAsync(context, cancellationToken);
    }

    private static async Task SeedUsersAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var users = await context.AppUsers.ToListAsync(cancellationToken);

        EnsureUser(
            context,
            users,
            fullName: "Andre Fernandes",
            email: "andre2411fernandes@gmail.com",
            role: UserRoles.Admin);

        EnsureUser(
            context,
            users,
            fullName: "Platform Admin",
            email: "admin@kitchenrecipes.local",
            role: UserRoles.Admin);

        var nextUserIndex = 1;
        while (users.Count < MinRowsPerTable)
        {
            EnsureUser(
                context,
                users,
                fullName: $"Demo User {nextUserIndex:00}",
                email: $"demo.user{nextUserIndex:00}@kitchenrecipes.local",
                role: UserRoles.User);

            nextUserIndex++;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureUser(
        ApplicationDbContext context,
        ICollection<AppUser> users,
        string fullName,
        string email,
        string role)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var existing = users.FirstOrDefault(x => x.Email == normalizedEmail);

        if (existing is not null)
        {
            existing.FullName = fullName;
            existing.Role = role;
            existing.IsActive = true;
            existing.UpdatedAtUtc = DateTime.UtcNow;
            return;
        }

        var user = new AppUser
        {
            FullName = fullName,
            Email = normalizedEmail,
            PasswordHash = HashPassword(DefaultPassword),
            Role = role,
            IsActive = true,
        };

        context.AppUsers.Add(user);
        users.Add(user);
    }

    private static async Task SeedSubscribersAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var subscribers = await context.NewsletterSubscribers.ToListAsync(cancellationToken);

        EnsureSubscriber(context, subscribers, "andre2411fernandes@gmail.com", isConfirmed: true);

        var nextIndex = 1;
        while (subscribers.Count < MinRowsPerTable)
        {
            EnsureSubscriber(context, subscribers, $"subscriber{nextIndex:00}@kitchenrecipes.local", isConfirmed: nextIndex % 2 == 0);
            nextIndex++;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureSubscriber(
        ApplicationDbContext context,
        ICollection<NewsletterSubscriber> subscribers,
        string email,
        bool isConfirmed)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var existing = subscribers.FirstOrDefault(x => x.Email == normalizedEmail);

        if (existing is not null)
        {
            existing.IsConfirmed = isConfirmed;
            existing.UpdatedAtUtc = DateTime.UtcNow;
            return;
        }

        var subscriber = new NewsletterSubscriber
        {
            Email = normalizedEmail,
            IsConfirmed = isConfirmed,
        };

        context.NewsletterSubscribers.Add(subscriber);
        subscribers.Add(subscriber);
    }

    private static async Task SeedPantryItemsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var pantryItems = await context.PantryItems.ToListAsync(cancellationToken);

        var defaults = new[]
        {
            ("Tomato", 6m, "units", (DateOnly?)DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))),
            ("Onion", 5m, "units", (DateOnly?)DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14))),
            ("Garlic", 12m, "cloves", (DateOnly?)DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20))),
            ("Olive Oil", 1m, "bottle", (DateOnly?)null),
            ("Rice", 2m, "kg", (DateOnly?)null),
            ("Pasta", 3m, "packs", (DateOnly?)null),
            ("Chicken Breast", 2m, "kg", (DateOnly?)DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3))),
            ("Eggs", 18m, "units", (DateOnly?)DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))),
            ("Milk", 2m, "liters", (DateOnly?)DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4))),
            ("Cheese", 1m, "kg", (DateOnly?)DateOnly.FromDateTime(DateTime.UtcNow.AddDays(9))),
            ("Spinach", 3m, "packs", (DateOnly?)DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))),
            ("Beans", 4m, "cans", (DateOnly?)null),
        };

        foreach (var item in defaults)
        {
            var existing = pantryItems.FirstOrDefault(x => x.Name.Equals(item.Item1, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                existing.AvailableQuantity = item.Item2;
                existing.Unit = item.Item3;
                existing.ExpirationDate = item.Item4;
                existing.UpdatedAtUtc = DateTime.UtcNow;
                continue;
            }

            var pantry = new PantryItem
            {
                Name = item.Item1,
                AvailableQuantity = item.Item2,
                Unit = item.Item3,
                ExpirationDate = item.Item4,
            };

            context.PantryItems.Add(pantry);
            pantryItems.Add(pantry);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedRecipesAndIngredientsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var recipes = await context.Recipes
            .Include(x => x.Ingredients)
            .ToListAsync(cancellationToken);

        var defaults = new[]
        {
            new RecipeSeed("Tomato Garlic Pasta", "Simple pasta with tomato and garlic sauce.", 20, 2, new[] { "Pasta", "Tomato", "Garlic", "Olive Oil" }),
            new RecipeSeed("Chicken Rice Bowl", "Protein-rich chicken and rice bowl.", 30, 3, new[] { "Chicken Breast", "Rice", "Onion", "Olive Oil" }),
            new RecipeSeed("Spinach Omelette", "Fast omelette with spinach and cheese.", 12, 1, new[] { "Eggs", "Spinach", "Cheese", "Milk" }),
            new RecipeSeed("Beans and Rice", "Budget meal with beans and rice.", 25, 4, new[] { "Beans", "Rice", "Onion", "Garlic" }),
            new RecipeSeed("Creamy Chicken Pasta", "Creamy pasta with chicken strips.", 28, 3, new[] { "Pasta", "Chicken Breast", "Milk", "Cheese" }),
            new RecipeSeed("Tomato Egg Scramble", "Egg scramble with tomato and onion.", 10, 2, new[] { "Eggs", "Tomato", "Onion", "Olive Oil" }),
            new RecipeSeed("Cheesy Rice Bake", "Oven-baked rice with cheese topping.", 35, 5, new[] { "Rice", "Cheese", "Milk", "Onion" }),
            new RecipeSeed("Garlic Spinach Chicken", "Skillet chicken with garlic spinach.", 22, 2, new[] { "Chicken Breast", "Garlic", "Spinach", "Olive Oil" }),
            new RecipeSeed("Bean Tomato Stew", "Warm bean and tomato stew.", 27, 4, new[] { "Beans", "Tomato", "Onion", "Garlic" }),
            new RecipeSeed("Breakfast Protein Plate", "Eggs with chicken and spinach.", 18, 2, new[] { "Eggs", "Chicken Breast", "Spinach", "Cheese" }),
        };

        foreach (var item in defaults)
        {
            var existing = recipes.FirstOrDefault(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                existing = new Recipe
                {
                    Name = item.Name,
                    Description = item.Description,
                    PreparationMinutes = item.PreparationMinutes,
                    Servings = item.Servings,
                };

                context.Recipes.Add(existing);
                recipes.Add(existing);
            }
            else
            {
                existing.Description = item.Description;
                existing.PreparationMinutes = item.PreparationMinutes;
                existing.Servings = item.Servings;
                existing.UpdatedAtUtc = DateTime.UtcNow;
            }

            foreach (var ingredientName in item.Ingredients)
            {
                var existingIngredient = existing.Ingredients.FirstOrDefault(x => x.Name.Equals(ingredientName, StringComparison.OrdinalIgnoreCase));
                if (existingIngredient is not null)
                {
                    continue;
                }

                existing.Ingredients.Add(new Ingredient
                {
                    Name = ingredientName,
                    Quantity = 1,
                    Unit = "portion",
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        var ingredientCount = await context.Ingredients.CountAsync(cancellationToken);
        if (ingredientCount >= MinRowsPerTable)
        {
            return;
        }

        var firstRecipe = await context.Recipes.Include(x => x.Ingredients).FirstAsync(cancellationToken);
        while (ingredientCount < MinRowsPerTable)
        {
            firstRecipe.Ingredients.Add(new Ingredient
            {
                Name = $"Extra Ingredient {ingredientCount + 1:00}",
                Quantity = 1,
                Unit = "portion",
            });
            ingredientCount++;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedCampaignAuditsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var count = await context.NewsletterCampaignAudits.CountAsync(cancellationToken);
        if (count >= MinRowsPerTable)
        {
            return;
        }

        var templates = new[] { "weekly_menu", "pantry_rescue", "seasonal_special", "healthy_starters", "community_challenge" };
        var missing = MinRowsPerTable - count;

        for (var i = 0; i < missing; i++)
        {
            var index = count + i;
            context.NewsletterCampaignAudits.Add(new NewsletterCampaignAudit
            {
                TemplateKey = templates[index % templates.Length],
                RecipientsCount = 10 + (index % 6),
                SentAtUtc = DateTime.UtcNow.AddDays(-index),
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedRoleAuditsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var hasAuditTable = await context.Database.SqlQueryRaw<int>(
            "SELECT CAST(CASE WHEN OBJECT_ID(N'[dbo].[AppUserRoleAudits]', N'U') IS NULL THEN 0 ELSE 1 END AS int) AS [Value]")
            .SingleAsync(cancellationToken);

        if (hasAuditTable == 0)
        {
            return;
        }

        var existingCount = await context.Database.SqlQueryRaw<int>("SELECT COUNT(1) AS [Value] FROM [dbo].[AppUserRoleAudits]")
            .SingleAsync(cancellationToken);

        if (existingCount >= MinRowsPerTable)
        {
            return;
        }

        var missing = MinRowsPerTable - existingCount;

        await context.Database.ExecuteSqlRawAsync(
            """
            ;WITH UserPool AS
            (
                SELECT TOP ({0}) [Id], ROW_NUMBER() OVER (ORDER BY [Id]) AS [RowNo]
                FROM [dbo].[AppUsers]
                ORDER BY [Id]
            )
            INSERT INTO [dbo].[AppUserRoleAudits] ([AppUserId], [OldRole], [NewRole], [ChangedAtUtc])
            SELECT
                [Id],
                'User',
                'Admin',
                DATEADD(MINUTE, -[RowNo], SYSUTCDATETIME())
            FROM UserPool;
            """,
            parameters: [missing],
            cancellationToken: cancellationToken);
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private sealed record RecipeSeed(
        string Name,
        string Description,
        int PreparationMinutes,
        int Servings,
        IReadOnlyList<string> Ingredients);
}
