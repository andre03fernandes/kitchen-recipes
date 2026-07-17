using KitchenRecipes.Application.Services;
using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Domain.Entities;

namespace KitchenRecipes.Tests;

public sealed class PantryAssistantServiceTests
{
    [Fact]
    public async Task GetSuggestionsAsync_ReturnsRankedSuggestionsFromPantryMatches()
    {
        await using var context = TestApplicationDbContextFactory.Create();

        context.PantryItems.AddRange(
            new PantryItem { Name = "Tomato", AvailableQuantity = 3, Unit = "units" },
            new PantryItem { Name = "Pasta", AvailableQuantity = 1, Unit = "pack" },
            new PantryItem { Name = "Garlic", AvailableQuantity = 5, Unit = "cloves" });

        var tomatoPasta = new Recipe
        {
            Name = "Tomato Pasta",
            Description = "Quick tomato pasta",
            PreparationMinutes = 15,
            Servings = 2,
            Ingredients =
            [
                new Ingredient { Name = "Tomato", Quantity = 2, Unit = "units" },
                new Ingredient { Name = "Pasta", Quantity = 1, Unit = "pack" },
                new Ingredient { Name = "Garlic", Quantity = 2, Unit = "cloves" }
            ]
        };

        var omelette = new Recipe
        {
            Name = "Cheese Omelette",
            Description = "Simple omelette",
            PreparationMinutes = 10,
            Servings = 1,
            Ingredients =
            [
                new Ingredient { Name = "Eggs", Quantity = 2, Unit = "units" },
                new Ingredient { Name = "Cheese", Quantity = 1, Unit = "slice" }
            ]
        };

        context.Recipes.AddRange(tomatoPasta, omelette);
        await context.SaveChangesAsync();

        var service = new PantryAssistantService(context);
        var result = await service.GetSuggestionsAsync();

        Assert.Equal(3, result.PantryItemsCount);
        Assert.Equal(2, result.RecipesEvaluated);
        Assert.Single(result.Suggestions);
        Assert.Equal("Tomato Pasta", result.Suggestions[0].RecipeName);
        Assert.Equal(1.00m, result.Suggestions[0].MatchScore);
        Assert.Equal(1.00m, result.Suggestions[0].QuantityCoverageScore);
        Assert.Empty(result.Suggestions[0].MissingIngredients);
        Assert.Empty(result.Suggestions[0].InsufficientIngredients);
    }

    [Fact]
    public async Task GetSuggestionsAsync_IncludesMissingIngredients_WhenRecipePartiallyMatches()
    {
        await using var context = TestApplicationDbContextFactory.Create();

        context.PantryItems.AddRange(
            new PantryItem { Name = "Chicken Breast", AvailableQuantity = 1, Unit = "kg" },
            new PantryItem { Name = "Rice", AvailableQuantity = 1, Unit = "kg" });

        context.Recipes.Add(new Recipe
        {
            Name = "Chicken Rice Bowl",
            Description = "Bowl recipe",
            PreparationMinutes = 25,
            Servings = 2,
            Ingredients =
            [
                new Ingredient { Name = "Chicken Breast", Quantity = 1, Unit = "kg" },
                new Ingredient { Name = "Rice", Quantity = 1, Unit = "kg" },
                new Ingredient { Name = "Onion", Quantity = 1, Unit = "unit" }
            ]
        });

        await context.SaveChangesAsync();

        var service = new PantryAssistantService(context);
        var result = await service.GetSuggestionsAsync();

        Assert.Single(result.Suggestions);
        Assert.Equal(0.67m, result.Suggestions[0].MatchScore);
        Assert.Equal(0.67m, result.Suggestions[0].QuantityCoverageScore);
        Assert.Contains(result.Suggestions[0].MissingIngredients, x => x.StartsWith("Onion"));
    }

    [Fact]
    public async Task GetSuggestionsAsync_ReducesScore_WhenMatchedIngredientStockIsInsufficient()
    {
        await using var context = TestApplicationDbContextFactory.Create();

        context.PantryItems.AddRange(
            new PantryItem { Name = "Tomato", AvailableQuantity = 1, Unit = "units" },
            new PantryItem { Name = "Pasta", AvailableQuantity = 1, Unit = "pack" },
            new PantryItem { Name = "Garlic", AvailableQuantity = 1, Unit = "cloves" });

        context.Recipes.Add(new Recipe
        {
            Name = "Tomato Pasta",
            Description = "Quick tomato pasta",
            PreparationMinutes = 15,
            Servings = 2,
            Ingredients =
            [
                new Ingredient { Name = "Tomato", Quantity = 2, Unit = "units" },
                new Ingredient { Name = "Pasta", Quantity = 1, Unit = "pack" },
                new Ingredient { Name = "Garlic", Quantity = 2, Unit = "cloves" }
            ]
        });

        await context.SaveChangesAsync();

        var service = new PantryAssistantService(context);
        var result = await service.GetSuggestionsAsync();

        Assert.Single(result.Suggestions);
        Assert.Equal(0.84m, result.Suggestions[0].MatchScore);
        Assert.Equal(0.67m, result.Suggestions[0].QuantityCoverageScore);
        Assert.Equal(2, result.Suggestions[0].InsufficientIngredients.Count);
        Assert.Contains(result.Suggestions[0].InsufficientIngredients, x => x.StartsWith("Tomato"));
    }

    [Fact]
    public async Task GetSuggestionsAsync_UsesWeightUnitConversion_ForCoverage()
    {
        await using var context = TestApplicationDbContextFactory.Create();

        context.PantryItems.Add(new PantryItem { Name = "Flour", AvailableQuantity = 1, Unit = "kg" });

        context.Recipes.Add(new Recipe
        {
            Name = "Flatbread",
            Description = "Simple bread",
            PreparationMinutes = 20,
            Servings = 4,
            Ingredients =
            [
                new Ingredient { Name = "Flour", Quantity = 500, Unit = "g" }
            ]
        });

        await context.SaveChangesAsync();

        var service = new PantryAssistantService(context);
        var result = await service.GetSuggestionsAsync();

        Assert.Single(result.Suggestions);
        Assert.Equal(1.00m, result.Suggestions[0].MatchScore);
        Assert.Equal(1.00m, result.Suggestions[0].QuantityCoverageScore);
        Assert.Contains(result.Suggestions[0].ConversionInsights, x => x.Contains("1 kg converts to 1000 g", StringComparison.Ordinal));
        Assert.Contains("used unit conversion", result.Suggestions[0].SuggestionReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetSuggestionsAsync_UsesVolumeUnitConversion_ForInsufficientCoverage()
    {
        await using var context = TestApplicationDbContextFactory.Create();

        context.PantryItems.Add(new PantryItem { Name = "Milk", AvailableQuantity = 1, Unit = "cup" });

        context.Recipes.Add(new Recipe
        {
            Name = "Milk Soup",
            Description = "Creamy soup",
            PreparationMinutes = 30,
            Servings = 3,
            Ingredients =
            [
                new Ingredient { Name = "Milk", Quantity = 300, Unit = "ml" }
            ]
        });

        await context.SaveChangesAsync();

        var service = new PantryAssistantService(context);
        var result = await service.GetSuggestionsAsync();

        Assert.Single(result.Suggestions);
        Assert.Equal(0.90m, result.Suggestions[0].MatchScore);
        Assert.Equal(0.80m, result.Suggestions[0].QuantityCoverageScore);
        Assert.Contains(result.Suggestions[0].InsufficientIngredients, x => x.Contains("240/300 ml", StringComparison.Ordinal));
        Assert.Contains(result.Suggestions[0].ConversionInsights, x => x.Contains("1 cup converts to 240 ml", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GetChatReplyAsync_ReturnsRankedRecipesAndFollowUpPrompts()
    {
        await using var context = TestApplicationDbContextFactory.Create();

        context.PantryItems.AddRange(
            new PantryItem { Name = "Pasta", AvailableQuantity = 2, Unit = "packs" },
            new PantryItem { Name = "Tomato", AvailableQuantity = 4, Unit = "units" },
            new PantryItem { Name = "Garlic", AvailableQuantity = 8, Unit = "cloves" },
            new PantryItem { Name = "Olive Oil", AvailableQuantity = 1, Unit = "bottle" });

        context.Recipes.AddRange(
            new Recipe
            {
                Name = "Quick Tomato Pasta",
                Description = "Fast pasta dinner with tomato and garlic.",
                PreparationMinutes = 15,
                Servings = 2,
                Ingredients =
                [
                    new Ingredient { Name = "Pasta", Quantity = 1, Unit = "pack" },
                    new Ingredient { Name = "Tomato", Quantity = 2, Unit = "units" },
                    new Ingredient { Name = "Garlic", Quantity = 2, Unit = "cloves" }
                ]
            },
            new Recipe
            {
                Name = "Bean Rice Bowl",
                Description = "Filling bowl with beans and rice.",
                PreparationMinutes = 25,
                Servings = 3,
                Ingredients =
                [
                    new Ingredient { Name = "Beans", Quantity = 1, Unit = "can" },
                    new Ingredient { Name = "Rice", Quantity = 1, Unit = "pack" }
                ]
            });

        await context.SaveChangesAsync();

        var service = new PantryAssistantService(context);
        var response = await service.GetChatReplyAsync(new PantryAssistantChatRequestDto("I want a quick pasta recipe from my fridge", 3));

        Assert.Equal("I want a quick pasta recipe from my fridge", response.UserMessage);
        Assert.NotEmpty(response.AssistantMessage);
        Assert.NotEmpty(response.PantryHighlights);
        Assert.NotEmpty(response.FollowUpPrompts);
        Assert.NotEmpty(response.SuggestedRecipes);
        Assert.Equal("Quick Tomato Pasta", response.SuggestedRecipes[0].RecipeName);
    }

    [Fact]
    public async Task GetChatReplyAsync_Throws_WhenMessageIsEmpty()
    {
        await using var context = TestApplicationDbContextFactory.Create();
        var service = new PantryAssistantService(context);

        var action = async () => await service.GetChatReplyAsync(new PantryAssistantChatRequestDto("   "));

        await Assert.ThrowsAsync<ArgumentException>(action);
    }
}
