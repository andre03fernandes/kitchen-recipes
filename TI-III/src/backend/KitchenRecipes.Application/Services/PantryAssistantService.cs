using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace KitchenRecipes.Application.Services;

public sealed class PantryAssistantService : IPantryAssistantService
{
    private sealed record UnitDefinition(
        string Family,
        decimal ToBaseFactor,
        string DisplayUnit);

    private sealed record PantrySnapshot(
        string Name,
        decimal AvailableQuantity,
        string Unit);

    private sealed record IngredientSnapshot(
        string Name,
        decimal Quantity,
        string Unit);

    private sealed record RecipeSnapshot(
        int Id,
        string Name,
        string Description,
        int PreparationMinutes,
        int Servings,
        List<IngredientSnapshot> Ingredients);

    private sealed record AssistantDataSnapshot(
        IReadOnlyList<PantrySnapshot> PantryItems,
        IReadOnlyList<string> PantryNormalized,
        IReadOnlyList<RecipeSnapshot> Recipes);

    private sealed record QuantityCoverageResult(
        decimal Coverage,
        decimal AvailableInIngredientUnit,
        bool UsedConversion);

    private static readonly IReadOnlyDictionary<string, UnitDefinition> UnitDefinitions =
        new Dictionary<string, UnitDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["g"] = new("weight", 1m, "g"),
            ["gram"] = new("weight", 1m, "g"),
            ["grams"] = new("weight", 1m, "g"),
            ["kg"] = new("weight", 1000m, "kg"),
            ["kilogram"] = new("weight", 1000m, "kg"),
            ["kilograms"] = new("weight", 1000m, "kg"),
            ["mg"] = new("weight", 0.001m, "mg"),
            ["milligram"] = new("weight", 0.001m, "mg"),
            ["milligrams"] = new("weight", 0.001m, "mg"),
            ["oz"] = new("weight", 28.3495m, "oz"),
            ["ounce"] = new("weight", 28.3495m, "oz"),
            ["ounces"] = new("weight", 28.3495m, "oz"),
            ["lb"] = new("weight", 453.592m, "lb"),
            ["lbs"] = new("weight", 453.592m, "lb"),
            ["pound"] = new("weight", 453.592m, "lb"),
            ["pounds"] = new("weight", 453.592m, "lb"),
            ["ml"] = new("volume", 1m, "ml"),
            ["milliliter"] = new("volume", 1m, "ml"),
            ["milliliters"] = new("volume", 1m, "ml"),
            ["millilitre"] = new("volume", 1m, "ml"),
            ["millilitres"] = new("volume", 1m, "ml"),
            ["l"] = new("volume", 1000m, "l"),
            ["liter"] = new("volume", 1000m, "l"),
            ["liters"] = new("volume", 1000m, "l"),
            ["litre"] = new("volume", 1000m, "l"),
            ["litres"] = new("volume", 1000m, "l"),
            ["tsp"] = new("volume", 4.92892m, "tsp"),
            ["teaspoon"] = new("volume", 4.92892m, "tsp"),
            ["teaspoons"] = new("volume", 4.92892m, "tsp"),
            ["tbsp"] = new("volume", 14.7868m, "tbsp"),
            ["tablespoon"] = new("volume", 14.7868m, "tbsp"),
            ["tablespoons"] = new("volume", 14.7868m, "tbsp"),
            ["cup"] = new("volume", 240m, "cup"),
            ["cups"] = new("volume", 240m, "cup"),
            ["unit"] = new("piece", 1m, "unit"),
            ["units"] = new("piece", 1m, "unit"),
            ["piece"] = new("piece", 1m, "piece"),
            ["pieces"] = new("piece", 1m, "piece"),
            ["item"] = new("piece", 1m, "item"),
            ["items"] = new("piece", 1m, "item"),
            ["clove"] = new("clove", 1m, "clove"),
            ["cloves"] = new("clove", 1m, "clove"),
            ["slice"] = new("slice", 1m, "slice"),
            ["slices"] = new("slice", 1m, "slice"),
            ["pack"] = new("pack", 1m, "pack"),
            ["packs"] = new("pack", 1m, "pack"),
            ["can"] = new("can", 1m, "can"),
            ["cans"] = new("can", 1m, "can")
        };

    private readonly IApplicationDbContext _context;
    private readonly IAssistantTextGenerator _assistantTextGenerator;

    private sealed class NullAssistantTextGenerator : IAssistantTextGenerator
    {
        public Task<string?> GenerateReplyAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<string?>(null);
        }

        public async IAsyncEnumerable<string> GenerateReplyStreamAsync(string systemPrompt, string userPrompt, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }

    public PantryAssistantService(IApplicationDbContext context, IAssistantTextGenerator? assistantTextGenerator = null)
    {
        _context = context;
        _assistantTextGenerator = assistantTextGenerator ?? new NullAssistantTextGenerator();
    }

    public async Task<PantryAssistantResultDto> GetSuggestionsAsync(int limit = 5, CancellationToken cancellationToken = default)
    {
        var normalizedLimit = limit <= 0 ? 5 : Math.Min(limit, 20);

        var data = await LoadAssistantDataAsync(cancellationToken);
        var suggestions = BuildSuggestions(data.Recipes, data.PantryItems, data.PantryNormalized, normalizedLimit);

        return new PantryAssistantResultDto(
            PantryItemsCount: data.PantryItems.Count,
            RecipesEvaluated: data.Recipes.Count,
            Suggestions: suggestions);
    }

    public async Task<PantryAssistantChatResponseDto> GetChatReplyAsync(
        PantryAssistantChatRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var draft = await BuildChatDraftAsync(request, cancellationToken);
        var assistantMessage = await TryGenerateAiMessageAsync(
            draft.UserMessage,
            draft.SystemPrompt,
            draft.FallbackMessage,
            cancellationToken);

        return new PantryAssistantChatResponseDto(
            UserMessage: draft.UserMessage,
            AssistantMessage: assistantMessage,
            PantryItemsCount: draft.PantryItemsCount,
            RecipesEvaluated: draft.RecipesEvaluated,
            PantryHighlights: draft.PantryHighlights,
            SuggestedRecipes: draft.SuggestedRecipes,
            FollowUpPrompts: draft.FollowUpPrompts);
    }

    public async Task<PantryAssistantChatDraftDto> BuildChatDraftAsync(
        PantryAssistantChatRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateChatRequest(request);

        var normalizedLimit = request.Limit <= 0 ? 3 : Math.Min(request.Limit, 6);
        var data = await LoadAssistantDataAsync(cancellationToken);
        var suggestions = BuildSuggestions(data.Recipes, data.PantryItems, data.PantryNormalized, 8);
        var rankedSuggestions = RankSuggestionsForPrompt(suggestions, data.Recipes, request.Message)
            .Take(normalizedLimit)
            .ToList();

        var pantryHighlights = data.PantryItems
            .OrderByDescending(x => x.AvailableQuantity)
            .Take(6)
            .Select(FormatPantryAvailability)
            .ToList();

        var userMessage = request.Message.Trim();
        var fallbackMessage = BuildAssistantMessage(userMessage, rankedSuggestions, pantryHighlights);
        var followUpPrompts = BuildFollowUpPrompts(userMessage, rankedSuggestions);
        var systemPrompt = BuildSystemPrompt(rankedSuggestions, pantryHighlights);

        return new PantryAssistantChatDraftDto(
            UserMessage: userMessage,
            SystemPrompt: systemPrompt,
            FallbackMessage: fallbackMessage,
            PantryItemsCount: data.PantryItems.Count,
            RecipesEvaluated: data.Recipes.Count,
            PantryHighlights: pantryHighlights,
            SuggestedRecipes: rankedSuggestions,
            FollowUpPrompts: followUpPrompts);
    }

    private async Task<string> TryGenerateAiMessageAsync(
        string userMessage,
        string systemPrompt,
        string fallbackMessage,
        CancellationToken cancellationToken)
    {
        var aiReply = await _assistantTextGenerator.GenerateReplyAsync(systemPrompt, userMessage, cancellationToken);
        return string.IsNullOrWhiteSpace(aiReply) ? fallbackMessage : aiReply.Trim();
    }

    private static string BuildSystemPrompt(
        IReadOnlyList<PantryRecipeSuggestionDto> suggestions,
        IReadOnlyList<string> pantryHighlights)
    {
        var builder = new StringBuilder();
        builder.AppendLine("You are the AI assistant for the full Kitchen Recipes website.");
        builder.AppendLine("You can help with: recipe ideas, pantry planning, newsletter/admin guidance, account and navigation help.");
        builder.AppendLine("Keep answers concise, practical, and friendly.");
        builder.AppendLine("When the user asks about cooking, use pantry and recipe data provided below.");
        builder.AppendLine("When the user asks about product usage, explain clearly which module/page to use.");
        builder.AppendLine("If stock is missing for a recipe, call it out briefly and suggest alternatives.");

        if (pantryHighlights.Count > 0)
        {
            builder.AppendLine($"Pantry highlights: {string.Join(", ", pantryHighlights)}");
        }

        if (suggestions.Count > 0)
        {
            builder.AppendLine("Top recipe options:");
            foreach (var suggestion in suggestions.Take(4))
            {
                builder.AppendLine(
                    $"- {suggestion.RecipeName}: match {Math.Round(suggestion.MatchScore * 100)}%, coverage {Math.Round(suggestion.QuantityCoverageScore * 100)}%, missing [{string.Join(", ", suggestion.MissingIngredients)}]");
            }
        }

        return builder.ToString();
    }

    private async Task<AssistantDataSnapshot> LoadAssistantDataAsync(CancellationToken cancellationToken)
    {
        var pantryItemRows = await _context.PantryItems
            .AsNoTracking()
            .Where(x => x.AvailableQuantity > 0)
            .Select(x => new
            {
                x.Name,
                x.AvailableQuantity,
                x.Unit
            })
            .ToListAsync(cancellationToken);

        var pantryItems = pantryItemRows
            .Select(x => new PantrySnapshot(
                (x.Name ?? string.Empty).Trim(),
                x.AvailableQuantity,
                (x.Unit ?? string.Empty).Trim()))
            .Where(x => x.Name.Length > 0)
            .ToList();

        var pantryNormalized = pantryItems
            .Select(x => Normalize(x.Name))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var recipes = await _context.Recipes
            .AsNoTracking()
            .Select(recipe => new RecipeSnapshot(
                recipe.Id,
                recipe.Name,
                recipe.Description,
                recipe.PreparationMinutes,
                recipe.Servings,
                recipe.Ingredients
                    .Select(ingredient => new IngredientSnapshot(
                        ingredient.Name,
                        ingredient.Quantity,
                        ingredient.Unit))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return new AssistantDataSnapshot(
            PantryItems: pantryItems,
            PantryNormalized: pantryNormalized,
            Recipes: recipes);
    }

    private static IReadOnlyList<PantryRecipeSuggestionDto> BuildSuggestions(
        IReadOnlyList<RecipeSnapshot> recipes,
        IReadOnlyList<PantrySnapshot> pantryItems,
        IReadOnlyList<string> pantryNormalized,
        int normalizedLimit)
    {
        return recipes
            .Select(recipe => BuildSuggestion(recipe, pantryItems, pantryNormalized))
            .Where(x => x is not null)
            .Select(x => x!)
            .OrderByDescending(x => x.MatchScore)
            .ThenByDescending(x => x.QuantityCoverageScore)
            .ThenBy(x => x.PreparationMinutes)
            .Take(normalizedLimit)
            .ToList();
    }

    private static IReadOnlyList<PantryRecipeSuggestionDto> RankSuggestionsForPrompt(
        IReadOnlyList<PantryRecipeSuggestionDto> suggestions,
        IReadOnlyList<RecipeSnapshot> recipes,
        string prompt)
    {
        var normalizedPrompt = Normalize(prompt);
        var promptTokens = ExtractPromptTokens(normalizedPrompt);

        return suggestions
            .Select(suggestion => new
            {
                Suggestion = suggestion,
                PromptScore = CalculatePromptScore(
                    suggestion,
                    recipes.First(recipe => recipe.Id == suggestion.RecipeId),
                    normalizedPrompt,
                    promptTokens)
            })
            .OrderByDescending(x => x.PromptScore)
            .ThenByDescending(x => x.Suggestion.MatchScore)
            .ThenByDescending(x => x.Suggestion.QuantityCoverageScore)
            .ThenBy(x => x.Suggestion.PreparationMinutes)
            .Select(x => x.Suggestion)
            .ToList();
    }

    private static PantryRecipeSuggestionDto? BuildSuggestion(
        RecipeSnapshot recipe,
        IReadOnlyList<PantrySnapshot> pantryItems,
        IReadOnlyList<string> pantryNormalized)
    {
        var ingredients = recipe.Ingredients
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .GroupBy(x => x.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First() with { Name = group.First().Name.Trim() })
            .ToList();

        if (ingredients.Count == 0)
        {
            return BuildKeywordBasedSuggestion(recipe, pantryItems, pantryNormalized);
        }

        var matchedIngredients = ingredients
            .Select(ingredient => new
            {
                Ingredient = ingredient,
                PantryMatch = pantryItems.FirstOrDefault(pantry => IsMatch(Normalize(pantry.Name), Normalize(ingredient.Name)))
            })
            .Where(x => x.PantryMatch is not null)
            .ToList();

        if (matchedIngredients.Count == 0)
        {
            return null;
        }

        var missingIngredientNames = ingredients
            .Where(ingredient => matchedIngredients.All(x => !string.Equals(x.Ingredient.Name, ingredient.Name, StringComparison.OrdinalIgnoreCase)))
            .Select(ingredient => FormatIngredientRequirement(ingredient))
            .ToList();

        var matchedPantryItems = matchedIngredients
            .Select(x => x.PantryMatch!)
            .Select(pantry => FormatPantryAvailability(pantry))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var sufficientCount = 0;
        var quantityCoverageTotal = 0m;
        var insufficientIngredients = new List<string>();
        var conversionInsights = new List<string>();

        foreach (var match in matchedIngredients)
        {
            var coverage = CalculateQuantityCoverage(match.PantryMatch!, match.Ingredient);
            quantityCoverageTotal += coverage.Coverage;

            if (coverage.UsedConversion)
            {
                conversionInsights.Add(
                    $"{match.Ingredient.Name}: {match.PantryMatch!.AvailableQuantity:0.##} {match.PantryMatch.Unit} converts to {coverage.AvailableInIngredientUnit:0.##} {match.Ingredient.Unit}.");
            }

            if (coverage.Coverage >= 1m)
            {
                sufficientCount++;
                continue;
            }

            var availableText = coverage.UsedConversion
                ? $"{coverage.AvailableInIngredientUnit:0.##}/{match.Ingredient.Quantity:0.##} {match.Ingredient.Unit}"
                : $"{match.PantryMatch!.AvailableQuantity:0.##}/{match.Ingredient.Quantity:0.##} {match.Ingredient.Unit}";

            insufficientIngredients.Add($"{match.Ingredient.Name} ({availableText})");
        }

        var presenceScore = (decimal)matchedIngredients.Count / ingredients.Count;
        var quantityCoverageScore = decimal.Round(quantityCoverageTotal / ingredients.Count, 2, MidpointRounding.AwayFromZero);
        var matchScore = decimal.Round((presenceScore + quantityCoverageScore) / 2m, 2, MidpointRounding.AwayFromZero);

        return new PantryRecipeSuggestionDto(
            RecipeId: recipe.Id,
            RecipeName: recipe.Name,
            RecipeDescription: recipe.Description,
            PreparationMinutes: recipe.PreparationMinutes,
            Servings: recipe.Servings,
            MatchScore: matchScore,
            QuantityCoverageScore: quantityCoverageScore,
            MatchedPantryItems: matchedPantryItems,
            MissingIngredients: missingIngredientNames,
            InsufficientIngredients: insufficientIngredients,
                ConversionInsights: conversionInsights,
                SuggestionReason: BuildSuggestionReason(matchedIngredients.Count, ingredients.Count, sufficientCount, conversionInsights.Count));
    }

        private static decimal CalculatePromptScore(
            PantryRecipeSuggestionDto suggestion,
            RecipeSnapshot recipe,
            string normalizedPrompt,
            IReadOnlyList<string> promptTokens)
        {
            if (string.IsNullOrWhiteSpace(normalizedPrompt))
            {
                return suggestion.MatchScore + suggestion.QuantityCoverageScore;
            }

            var recipeText = Normalize($"{recipe.Name} {recipe.Description} {string.Join(' ', recipe.Ingredients.Select(x => x.Name))}");
            var matchedTokens = promptTokens.Count(token => recipeText.Contains(token, StringComparison.OrdinalIgnoreCase));
            var tokenScore = promptTokens.Count == 0 ? 0m : (decimal)matchedTokens / promptTokens.Count;
            var quickBonus = WantsQuickRecipe(normalizedPrompt) && recipe.PreparationMinutes <= 20 ? 0.35m : 0m;
            var proteinBonus = WantsProtein(normalizedPrompt) && HasProteinRecipe(recipe) ? 0.25m : 0m;
            var comfortingBonus = WantsComfortFood(normalizedPrompt) && HasComfortRecipe(recipe) ? 0.15m : 0m;

            return tokenScore + suggestion.MatchScore + suggestion.QuantityCoverageScore + quickBonus + proteinBonus + comfortingBonus;
        }

        private static List<string> ExtractPromptTokens(string normalizedPrompt)
        {
            return normalizedPrompt
                .Split([' ', ',', '.', ';', ':', '!', '?', '/', '\\', '-', '_'], StringSplitOptions.RemoveEmptyEntries)
                .Where(token => token.Length >= 3)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string BuildAssistantMessage(
            string userMessage,
            IReadOnlyList<PantryRecipeSuggestionDto> suggestions,
            IReadOnlyList<string> pantryHighlights)
        {
            if (suggestions.Count == 0)
            {
                return $"I can help across the whole Kitchen Recipes site. For cooking requests, I could not find a strong pantry recipe match for '{userMessage.Trim()}'. Try adding more pantry items, or ask me about recipes, pantry management, newsletter campaigns, or account/admin actions.";
            }

            var builder = new StringBuilder();
            builder.Append($"For '{userMessage.Trim()}', here is guidance as your site-wide assistant. ");
            builder.Append($"From your pantry context, your strongest ingredients right now are {string.Join(", ", pantryHighlights.Take(4))}. ");

            if (suggestions.Count == 1)
            {
                var top = suggestions[0];
                builder.Append($"My best match is {top.RecipeName} with {Math.Round(top.MatchScore * 100)}% overall match and {Math.Round(top.QuantityCoverageScore * 100)}% stock coverage.");
                return builder.ToString();
            }

            builder.Append($"My top matches are {string.Join(", ", suggestions.Take(3).Select(x => x.RecipeName))}. ");
            builder.Append($"I ranked them by pantry fit, stock coverage, and the style you asked for.");
            return builder.ToString();
        }

        private static IReadOnlyList<string> BuildFollowUpPrompts(
            string userMessage,
            IReadOnlyList<PantryRecipeSuggestionDto> suggestions)
        {
            var prompts = new List<string>
            {
                "Give me the quickest option under 20 minutes.",
                "Show me a higher-protein recipe from my fridge.",
                "Which recipe has the fewest missing ingredients?",
                "Guide me through the best workflow across recipes, pantry, and newsletter."
            };

            if (suggestions.Count > 0)
            {
                prompts.Add($"Tell me how to cook {suggestions[0].RecipeName} using what I already have.");
            }

            if (WantsQuickRecipe(Normalize(userMessage)))
            {
                prompts.Add("Do I have a more filling dinner option?");
            }

            return prompts
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(4)
                .ToList();
        }

        private static void ValidateChatRequest(PantryAssistantChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new ArgumentException("Please write what kind of recipe you want from your fridge.");
            }
        }

        private static bool WantsQuickRecipe(string normalizedPrompt)
        {
            return normalizedPrompt.Contains("quick", StringComparison.OrdinalIgnoreCase)
                || normalizedPrompt.Contains("fast", StringComparison.OrdinalIgnoreCase)
                || normalizedPrompt.Contains("20 minutes", StringComparison.OrdinalIgnoreCase)
                || normalizedPrompt.Contains("rapid", StringComparison.OrdinalIgnoreCase);
        }

        private static bool WantsProtein(string normalizedPrompt)
        {
            return normalizedPrompt.Contains("protein", StringComparison.OrdinalIgnoreCase)
                || normalizedPrompt.Contains("chicken", StringComparison.OrdinalIgnoreCase)
                || normalizedPrompt.Contains("eggs", StringComparison.OrdinalIgnoreCase)
                || normalizedPrompt.Contains("high-protein", StringComparison.OrdinalIgnoreCase);
        }

        private static bool WantsComfortFood(string normalizedPrompt)
        {
            return normalizedPrompt.Contains("comfort", StringComparison.OrdinalIgnoreCase)
                || normalizedPrompt.Contains("creamy", StringComparison.OrdinalIgnoreCase)
                || normalizedPrompt.Contains("pasta", StringComparison.OrdinalIgnoreCase)
                || normalizedPrompt.Contains("warm", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasProteinRecipe(RecipeSnapshot recipe)
        {
            var recipeText = Normalize($"{recipe.Name} {recipe.Description} {string.Join(' ', recipe.Ingredients.Select(x => x.Name))}");
            return recipeText.Contains("chicken", StringComparison.OrdinalIgnoreCase)
                || recipeText.Contains("egg", StringComparison.OrdinalIgnoreCase)
                || recipeText.Contains("beans", StringComparison.OrdinalIgnoreCase)
                || recipeText.Contains("cheese", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasComfortRecipe(RecipeSnapshot recipe)
        {
            var recipeText = Normalize($"{recipe.Name} {recipe.Description}");
            return recipeText.Contains("pasta", StringComparison.OrdinalIgnoreCase)
                || recipeText.Contains("creamy", StringComparison.OrdinalIgnoreCase)
                || recipeText.Contains("stew", StringComparison.OrdinalIgnoreCase)
                || recipeText.Contains("soup", StringComparison.OrdinalIgnoreCase);
        }

    private static PantryRecipeSuggestionDto? BuildKeywordBasedSuggestion(
        RecipeSnapshot recipe,
        IReadOnlyList<PantrySnapshot> pantryItems,
        IReadOnlyList<string> pantryNormalized)
    {
        var recipeText = Normalize($"{recipe.Name} {recipe.Description}");

        var matchedPantryItems = pantryItems
            .Where(pantry => IsMatch(recipeText, Normalize(pantry.Name)))
            .Select(FormatPantryAvailability)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (matchedPantryItems.Count == 0)
        {
            return null;
        }

        var denominator = Math.Max(pantryNormalized.Count, 1);
        var matchScore = decimal.Round((decimal)matchedPantryItems.Count / denominator, 2, MidpointRounding.AwayFromZero);

        return new PantryRecipeSuggestionDto(
            RecipeId: recipe.Id,
            RecipeName: recipe.Name,
            RecipeDescription: recipe.Description,
            PreparationMinutes: recipe.PreparationMinutes,
            Servings: recipe.Servings,
            MatchScore: matchScore,
            QuantityCoverageScore: 0m,
            MatchedPantryItems: matchedPantryItems,
            MissingIngredients: Array.Empty<string>(),
            InsufficientIngredients: Array.Empty<string>(),
            ConversionInsights: Array.Empty<string>(),
            SuggestionReason: "Matched by pantry keywords from recipe title/description.");
    }

    private static QuantityCoverageResult CalculateQuantityCoverage(PantrySnapshot pantry, IngredientSnapshot ingredient)
    {
        if (ingredient.Quantity <= 0)
        {
            return new QuantityCoverageResult(1m, pantry.AvailableQuantity, UsedConversion: false);
        }

        if (!TryConvertQuantity(pantry.AvailableQuantity, pantry.Unit, ingredient.Unit, out var availableInIngredientUnit, out var usedConversion))
        {
            return new QuantityCoverageResult(0.5m, pantry.AvailableQuantity, UsedConversion: false);
        }

        var ratio = availableInIngredientUnit / ingredient.Quantity;
        var coverage = decimal.Clamp(decimal.Round(ratio, 2, MidpointRounding.AwayFromZero), 0m, 1m);
        return new QuantityCoverageResult(coverage, decimal.Round(availableInIngredientUnit, 2, MidpointRounding.AwayFromZero), usedConversion);
    }

    private static bool TryConvertQuantity(
        decimal quantity,
        string fromUnit,
        string toUnit,
        out decimal convertedQuantity,
        out bool usedConversion)
    {
        convertedQuantity = quantity;
        usedConversion = false;

        var normalizedFromUnit = NormalizeUnit(fromUnit);
        var normalizedToUnit = NormalizeUnit(toUnit);

        if (normalizedFromUnit == normalizedToUnit)
        {
            return true;
        }

        if (!UnitDefinitions.TryGetValue(normalizedFromUnit, out var fromDefinition)
            || !UnitDefinitions.TryGetValue(normalizedToUnit, out var toDefinition)
            || !string.Equals(fromDefinition.Family, toDefinition.Family, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        convertedQuantity = quantity * fromDefinition.ToBaseFactor / toDefinition.ToBaseFactor;
        usedConversion = true;
        return true;
    }

    private static string NormalizeUnit(string unit)
    {
        var normalized = Normalize(unit);
        if (normalized.EndsWith("s", StringComparison.Ordinal) && normalized.Length > 1)
        {
            normalized = normalized[..^1];
        }

        return normalized;
    }

    private static string FormatIngredientRequirement(IngredientSnapshot ingredient)
    {
        return $"{ingredient.Name} ({ingredient.Quantity:0.##} {ingredient.Unit})";
    }

    private static string FormatPantryAvailability(PantrySnapshot pantry)
    {
        return $"{pantry.Name} ({pantry.AvailableQuantity:0.##} {pantry.Unit})";
    }

    private static string BuildSuggestionReason(int matchedCount, int ingredientCount, int sufficientCount, int conversionCount)
    {
        var reason = $"{matchedCount}/{ingredientCount} ingredients matched, {sufficientCount}/{ingredientCount} fully covered by stock.";

        if (conversionCount > 0)
        {
            reason += $" {conversionCount} match(es) used unit conversion.";
        }

        return reason;
    }

    private static bool IsMatch(string left, string right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return false;
        }

        return left.Contains(right, StringComparison.OrdinalIgnoreCase)
            || right.Contains(left, StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant();
    }
}
