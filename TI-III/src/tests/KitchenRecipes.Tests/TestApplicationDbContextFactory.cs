using KitchenRecipes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KitchenRecipes.Tests;

internal static class TestApplicationDbContextFactory
{
    public static ApplicationDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
