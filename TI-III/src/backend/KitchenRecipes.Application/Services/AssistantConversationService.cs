using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Text;
using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KitchenRecipes.Application.Services;

public sealed class AssistantConversationService : IAssistantConversationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IApplicationDbContext _context;
    private readonly IPantryAssistantService _pantryAssistantService;
    private readonly IAssistantTextGenerator _assistantTextGenerator;

    public AssistantConversationService(
        IApplicationDbContext context,
        IPantryAssistantService pantryAssistantService,
        IAssistantTextGenerator assistantTextGenerator)
    {
        _context = context;
        _pantryAssistantService = pantryAssistantService;
        _assistantTextGenerator = assistantTextGenerator;
    }

    public async Task<IReadOnlyList<AssistantChatThreadSummaryDto>> GetThreadsAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.AssistantChatThreads
            .AsNoTracking()
            .Where(x => x.AppUserId == userId)
            .OrderByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc)
            .Select(x => new AssistantChatThreadSummaryDto(
                x.Id,
                x.Title,
                x.LastMessagePreview,
                x.CreatedAtUtc,
                x.UpdatedAtUtc ?? x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<AssistantChatThreadDto?> GetThreadAsync(int userId, int threadId, CancellationToken cancellationToken = default)
    {
        var thread = await _context.AssistantChatThreads
            .AsNoTracking()
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == threadId && x.AppUserId == userId, cancellationToken);

        if (thread is null)
        {
            return null;
        }

        var messages = thread.Messages
            .OrderBy(x => x.CreatedAtUtc)
            .Select(MapMessage)
            .ToList();

        return new AssistantChatThreadDto(
            thread.Id,
            thread.Title,
            thread.CreatedAtUtc,
            thread.UpdatedAtUtc ?? thread.CreatedAtUtc,
            messages);
    }

    public async Task<AssistantChatExchangeDto> SendMessageAsync(int userId, SendAssistantChatMessageRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException("Message is required.");
        }

        var thread = await ResolveThreadAsync(userId, request.ThreadId, request.Message, cancellationToken);

        var userMessage = new AssistantChatMessage
        {
            AssistantChatThreadId = thread.Id,
            Role = "user",
            Content = request.Message.Trim(),
        };

        _context.AssistantChatMessages.Add(userMessage);

        var assistantDraft = await _pantryAssistantService.BuildChatDraftAsync(
            new PantryAssistantChatRequestDto(request.Message.Trim(), Limit: 4),
            cancellationToken);

        var userPromptWithContext = await BuildUserPromptWithContextAsync(
            thread.Id,
            assistantDraft.UserMessage,
            includeCurrentMessage: true,
            cancellationToken);

        var aiReply = await _assistantTextGenerator.GenerateReplyAsync(
            assistantDraft.SystemPrompt,
            userPromptWithContext,
            cancellationToken);

        var assistantContent = string.IsNullOrWhiteSpace(aiReply)
            ? assistantDraft.FallbackMessage
            : aiReply.Trim();

        var assistantMessage = new AssistantChatMessage
        {
            AssistantChatThreadId = thread.Id,
            Role = "assistant",
            Content = assistantContent,
            PantryHighlightsJson = JsonSerializer.Serialize(assistantDraft.PantryHighlights, JsonOptions),
            SuggestedRecipesJson = JsonSerializer.Serialize(assistantDraft.SuggestedRecipes, JsonOptions),
            FollowUpPromptsJson = JsonSerializer.Serialize(assistantDraft.FollowUpPrompts, JsonOptions),
        };

        _context.AssistantChatMessages.Add(assistantMessage);

        thread.LastMessagePreview = BuildPreview(assistantContent);
        thread.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var threadSummary = new AssistantChatThreadSummaryDto(
            thread.Id,
            thread.Title,
            thread.LastMessagePreview,
            thread.CreatedAtUtc,
            thread.UpdatedAtUtc ?? thread.CreatedAtUtc);

        return new AssistantChatExchangeDto(threadSummary, MapMessage(userMessage), MapMessage(assistantMessage));
    }

    public async IAsyncEnumerable<AssistantChatStreamEventDto> StreamMessageAsync(
        int userId,
        SendAssistantChatMessageRequestDto request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException("Message is required.");
        }

        var thread = await ResolveThreadAsync(userId, request.ThreadId, request.Message, cancellationToken);

        var userMessage = new AssistantChatMessage
        {
            AssistantChatThreadId = thread.Id,
            Role = "user",
            Content = request.Message.Trim(),
        };

        _context.AssistantChatMessages.Add(userMessage);
        await _context.SaveChangesAsync(cancellationToken);

        var assistantDraft = await _pantryAssistantService.BuildChatDraftAsync(
            new PantryAssistantChatRequestDto(request.Message.Trim(), Limit: 4),
            cancellationToken);

        var userPromptWithContext = await BuildUserPromptWithContextAsync(
            thread.Id,
            assistantDraft.UserMessage,
            includeCurrentMessage: false,
            cancellationToken);

        var threadSummary = new AssistantChatThreadSummaryDto(
            thread.Id,
            thread.Title,
            thread.LastMessagePreview,
            thread.CreatedAtUtc,
            thread.UpdatedAtUtc ?? thread.CreatedAtUtc);

        yield return new AssistantChatStreamEventDto(
            Type: "thread",
            Token: null,
            Thread: threadSummary,
            UserMessage: MapMessage(userMessage),
            AssistantMessage: null);

        var streamedBuilder = new StringBuilder();
        await foreach (var token in _assistantTextGenerator.GenerateReplyStreamAsync(
            assistantDraft.SystemPrompt,
            userPromptWithContext,
            cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(token))
            {
                continue;
            }

            streamedBuilder.Append(token);
            yield return new AssistantChatStreamEventDto(
                Type: "token",
                Token: token,
                Thread: null,
                UserMessage: null,
                AssistantMessage: null);
        }

        var streamedContent = streamedBuilder.ToString().Trim();
        var finalAssistantContent = string.IsNullOrWhiteSpace(streamedContent)
            ? assistantDraft.FallbackMessage
            : streamedContent;

        var assistantMessage = new AssistantChatMessage
        {
            AssistantChatThreadId = thread.Id,
            Role = "assistant",
            Content = finalAssistantContent,
            PantryHighlightsJson = JsonSerializer.Serialize(assistantDraft.PantryHighlights, JsonOptions),
            SuggestedRecipesJson = JsonSerializer.Serialize(assistantDraft.SuggestedRecipes, JsonOptions),
            FollowUpPromptsJson = JsonSerializer.Serialize(assistantDraft.FollowUpPrompts, JsonOptions),
        };

        _context.AssistantChatMessages.Add(assistantMessage);
        thread.LastMessagePreview = BuildPreview(finalAssistantContent);
        thread.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var updatedThread = new AssistantChatThreadSummaryDto(
            thread.Id,
            thread.Title,
            thread.LastMessagePreview,
            thread.CreatedAtUtc,
            thread.UpdatedAtUtc ?? thread.CreatedAtUtc);

        yield return new AssistantChatStreamEventDto(
            Type: "done",
            Token: null,
            Thread: updatedThread,
            UserMessage: null,
            AssistantMessage: MapMessage(assistantMessage));
    }

    public async Task<bool> RenameThreadAsync(int userId, int threadId, string title, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.");
        }

        var thread = await _context.AssistantChatThreads
            .FirstOrDefaultAsync(x => x.Id == threadId && x.AppUserId == userId, cancellationToken);

        if (thread is null)
        {
            return false;
        }

        thread.Title = BuildThreadTitle(title.Trim());
        thread.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteThreadAsync(int userId, int threadId, CancellationToken cancellationToken = default)
    {
        var thread = await _context.AssistantChatThreads
            .FirstOrDefaultAsync(x => x.Id == threadId && x.AppUserId == userId, cancellationToken);

        if (thread is null)
        {
            return false;
        }

        _context.AssistantChatThreads.Remove(thread);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AssistantChatThread> ResolveThreadAsync(int userId, int? threadId, string firstMessage, CancellationToken cancellationToken)
    {
        if (threadId is int id)
        {
            var existing = await _context.AssistantChatThreads
                .FirstOrDefaultAsync(x => x.Id == id && x.AppUserId == userId, cancellationToken);

            if (existing is null)
            {
                throw new ArgumentException("Chat thread was not found.");
            }

            return existing;
        }

        var title = BuildThreadTitle(firstMessage);
        var thread = new AssistantChatThread
        {
            AppUserId = userId,
            Title = title,
            LastMessagePreview = BuildPreview(firstMessage),
        };

        _context.AssistantChatThreads.Add(thread);
        await _context.SaveChangesAsync(cancellationToken);
        return thread;
    }

    private async Task<string> BuildUserPromptWithContextAsync(
        int threadId,
        string currentUserMessage,
        bool includeCurrentMessage,
        CancellationToken cancellationToken)
    {
        var recentMessages = await _context.AssistantChatMessages
            .AsNoTracking()
            .Where(x => x.AssistantChatThreadId == threadId && !string.IsNullOrWhiteSpace(x.Content))
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Take(10)
            .Select(x => new
            {
                x.Role,
                x.Content
            })
            .ToListAsync(cancellationToken);

        recentMessages.Reverse();

        var builder = new StringBuilder();
        if (recentMessages.Count > 0)
        {
            builder.AppendLine("Conversation so far:");
            foreach (var message in recentMessages)
            {
                var role = string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase)
                    ? "Assistant"
                    : "User";

                builder.Append(role)
                    .Append(": ")
                    .AppendLine(message.Content.Trim());
            }

            builder.AppendLine();
        }

        if (includeCurrentMessage)
        {
            builder.Append("Current user message: ")
                .Append(currentUserMessage.Trim());
            return builder.ToString();
        }

        if (recentMessages.Count == 0)
        {
            return currentUserMessage.Trim();
        }

        return builder.ToString().TrimEnd();
    }

    private static AssistantChatMessageDto MapMessage(AssistantChatMessage message)
    {
        return new AssistantChatMessageDto(
            message.Id,
            message.Role,
            message.Content,
            message.CreatedAtUtc,
            ParseList<string>(message.PantryHighlightsJson),
            ParseList<PantryRecipeSuggestionDto>(message.SuggestedRecipesJson),
            ParseList<string>(message.FollowUpPromptsJson));
    }

    private static IReadOnlyList<T> ParseList<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<T>();
        }

        var values = JsonSerializer.Deserialize<List<T>>(json, JsonOptions);
        if (values is null)
        {
            return Array.Empty<T>();
        }

        return values;
    }

    private static string BuildThreadTitle(string message)
    {
        var normalized = message.Trim();
        if (normalized.Length <= 56)
        {
            return normalized;
        }

        return string.Concat(normalized[..56].TrimEnd(), "...");
    }

    private static string BuildPreview(string message)
    {
        var normalized = message.Trim();
        if (normalized.Length <= 120)
        {
            return normalized;
        }

        return string.Concat(normalized[..120].TrimEnd(), "...");
    }

}
