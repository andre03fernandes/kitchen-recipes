using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Runtime.CompilerServices;
using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KitchenRecipes.Infrastructure.Services;

public sealed class OllamaAssistantTextGenerator : IAssistantTextGenerator
{
    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;
    private readonly IAiProfilePreferenceService _profilePreferenceService;
    private readonly ILogger<OllamaAssistantTextGenerator> _logger;

    public OllamaAssistantTextGenerator(
        HttpClient httpClient,
        IOptions<AiOptions> options,
        IAiProfilePreferenceService profilePreferenceService,
        ILogger<OllamaAssistantTextGenerator> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _profilePreferenceService = profilePreferenceService;
        _logger = logger;
    }

    public async Task<string?> GenerateReplyAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || !string.Equals(_options.Provider, "ollama", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var settings = _profilePreferenceService.GetCurrent();
        var request = BuildChatRequest(settings.ActiveModel, systemPrompt, userPrompt, stream: false);

        try
        {
            using var message = BuildHttpRequest("/api/chat", request);
            using var response = await _httpClient.SendAsync(message, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Ollama chat request failed with status code {StatusCode}.", response.StatusCode);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken: cancellationToken);
            return payload?.Message?.Content?.Trim();
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "Ollama chat request failed for model {Model}.", settings.ActiveModel);
            return null;
        }
    }

    public async IAsyncEnumerable<string> GenerateReplyStreamAsync(
        string systemPrompt,
        string userPrompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || !string.Equals(_options.Provider, "ollama", StringComparison.OrdinalIgnoreCase))
        {
            yield break;
        }

        var settings = _profilePreferenceService.GetCurrent();

        var request = BuildChatRequest(settings.ActiveModel, systemPrompt, userPrompt, stream: true);

        using var response = await TrySendStreamingRequestAsync(request, settings.ActiveModel, cancellationToken);
        if (response is null)
        {
            yield break;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            OllamaStreamChunk? chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<OllamaStreamChunk>(line);
            }
            catch (JsonException)
            {
                continue;
            }

            var token = chunk?.Message?.Content;
            if (!string.IsNullOrEmpty(token))
            {
                yield return token;
            }

            if (chunk?.Done == true)
            {
                yield break;
            }
        }
    }

    private async Task<HttpResponseMessage?> TrySendStreamingRequestAsync(
        OllamaChatRequest request,
        string model,
        CancellationToken cancellationToken)
    {
        try
        {
            using var message = BuildHttpRequest("/api/chat", request);
            var response = await _httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Ollama stream request failed with status code {StatusCode}.", response.StatusCode);
                response.Dispose();
                return null;
            }

            return response;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "Ollama stream request failed for model {Model}.", model);
            return null;
        }
    }

    private HttpRequestMessage BuildHttpRequest(string endpoint, OllamaChatRequest request)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(request)
        };

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        return message;
    }

    private static OllamaChatRequest BuildChatRequest(string model, string systemPrompt, string userPrompt, bool stream)
    {
        var messages = new List<OllamaChatMessage>
        {
            new("system", systemPrompt),
            new("user", userPrompt)
        };

        return new OllamaChatRequest(
            Model: model,
            Messages: messages,
            Stream: stream,
            Options: new OllamaGenerationOptions(Temperature: 0.85m, TopP: 0.92m, RepeatPenalty: 1.18m));
    }

    private sealed record OllamaChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<OllamaChatMessage> Messages,
        [property: JsonPropertyName("stream")] bool Stream,
        [property: JsonPropertyName("options")] OllamaGenerationOptions Options);

    private sealed record OllamaChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record OllamaChatResponse(
        [property: JsonPropertyName("message")] OllamaChatMessage? Message);

    private sealed record OllamaGenerationOptions(
        [property: JsonPropertyName("temperature")] decimal Temperature,
        [property: JsonPropertyName("top_p")] decimal TopP,
        [property: JsonPropertyName("repeat_penalty")] decimal RepeatPenalty);

    private sealed record OllamaStreamChunk(
        [property: JsonPropertyName("message")] OllamaChatMessage? Message,
        [property: JsonPropertyName("done")] bool Done);
}
