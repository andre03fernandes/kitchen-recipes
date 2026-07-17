namespace KitchenRecipes.Application.DTOs;

public sealed record AssistantChatThreadSummaryDto(
    int Id,
    string Title,
    string LastMessagePreview,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record AssistantChatMessageDto(
    int Id,
    string Role,
    string Content,
    DateTime CreatedAtUtc,
    IReadOnlyList<string> PantryHighlights,
    IReadOnlyList<PantryRecipeSuggestionDto> SuggestedRecipes,
    IReadOnlyList<string> FollowUpPrompts);

public sealed record AssistantChatThreadDto(
    int Id,
    string Title,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<AssistantChatMessageDto> Messages);

public sealed record SendAssistantChatMessageRequestDto(
    int? ThreadId,
    string Message);

public sealed record RenameAssistantChatThreadRequestDto(string Title);

public sealed record AssistantChatExchangeDto(
    AssistantChatThreadSummaryDto Thread,
    AssistantChatMessageDto UserMessage,
    AssistantChatMessageDto AssistantMessage);

public sealed record AssistantChatStreamEventDto(
    string Type,
    string? Token,
    AssistantChatThreadSummaryDto? Thread,
    AssistantChatMessageDto? UserMessage,
    AssistantChatMessageDto? AssistantMessage);

public sealed record UpdateAiProfileRequestDto(string Profile);

public sealed record AiProfileSettingsDto(
    bool Enabled,
    string Provider,
    string ActiveProfile,
    string FastModel,
    string QualityModel,
    string ActiveModel);
