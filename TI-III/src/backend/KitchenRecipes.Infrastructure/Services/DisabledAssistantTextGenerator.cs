using KitchenRecipes.Application.Abstractions;

namespace KitchenRecipes.Infrastructure.Services;

public sealed class DisabledAssistantTextGenerator : IAssistantTextGenerator
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
