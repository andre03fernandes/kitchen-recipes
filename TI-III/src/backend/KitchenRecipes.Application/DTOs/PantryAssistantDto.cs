namespace KitchenRecipes.Application.DTOs;

public sealed record PantryAssistantChatRequestDto(
    string Message,
    int Limit = 3);

public sealed record PantryAssistantChatResponseDto(
    string UserMessage,
    string AssistantMessage,
    int PantryItemsCount,
    int RecipesEvaluated,
    IReadOnlyList<string> PantryHighlights,
    IReadOnlyList<PantryRecipeSuggestionDto> SuggestedRecipes,
    IReadOnlyList<string> FollowUpPrompts);

public sealed record PantryAssistantChatDraftDto(
    string UserMessage,
    string SystemPrompt,
    string FallbackMessage,
    int PantryItemsCount,
    int RecipesEvaluated,
    IReadOnlyList<string> PantryHighlights,
    IReadOnlyList<PantryRecipeSuggestionDto> SuggestedRecipes,
    IReadOnlyList<string> FollowUpPrompts);

public sealed record PantryRecipeSuggestionDto(
    int RecipeId,
    string RecipeName,
    string RecipeDescription,
    int PreparationMinutes,
    int Servings,
    decimal MatchScore,
    decimal QuantityCoverageScore,
    IReadOnlyList<string> MatchedPantryItems,
    IReadOnlyList<string> MissingIngredients,
    IReadOnlyList<string> InsufficientIngredients,
    IReadOnlyList<string> ConversionInsights,
    string SuggestionReason);

public sealed record PantryAssistantResultDto(
    int PantryItemsCount,
    int RecipesEvaluated,
    IReadOnlyList<PantryRecipeSuggestionDto> Suggestions);
