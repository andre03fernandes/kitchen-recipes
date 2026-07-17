using KitchenRecipes.Application.DTOs;

namespace KitchenRecipes.Application.Abstractions;

public interface IPantryAssistantService
{
    Task<PantryAssistantResultDto> GetSuggestionsAsync(int limit = 5, CancellationToken cancellationToken = default);
    Task<PantryAssistantChatResponseDto> GetChatReplyAsync(PantryAssistantChatRequestDto request, CancellationToken cancellationToken = default);
    Task<PantryAssistantChatDraftDto> BuildChatDraftAsync(PantryAssistantChatRequestDto request, CancellationToken cancellationToken = default);
}
