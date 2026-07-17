namespace KitchenRecipes.Application.Abstractions;

public interface IAssistantTextGenerator
{
    Task<string?> GenerateReplyAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> GenerateReplyStreamAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
