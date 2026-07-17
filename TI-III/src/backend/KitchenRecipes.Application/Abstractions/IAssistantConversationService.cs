using KitchenRecipes.Application.DTOs;

namespace KitchenRecipes.Application.Abstractions;

public interface IAssistantConversationService
{
    Task<IReadOnlyList<AssistantChatThreadSummaryDto>> GetThreadsAsync(int userId, CancellationToken cancellationToken = default);
    Task<AssistantChatThreadDto?> GetThreadAsync(int userId, int threadId, CancellationToken cancellationToken = default);
    Task<AssistantChatExchangeDto> SendMessageAsync(int userId, SendAssistantChatMessageRequestDto request, CancellationToken cancellationToken = default);
    IAsyncEnumerable<AssistantChatStreamEventDto> StreamMessageAsync(int userId, SendAssistantChatMessageRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> RenameThreadAsync(int userId, int threadId, string title, CancellationToken cancellationToken = default);
    Task<bool> DeleteThreadAsync(int userId, int threadId, CancellationToken cancellationToken = default);
}
