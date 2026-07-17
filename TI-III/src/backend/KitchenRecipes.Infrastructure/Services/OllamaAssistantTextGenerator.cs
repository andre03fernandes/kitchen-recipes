using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Runtime.CompilerServices;
using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace KitchenRecipes.Infrastructure.Services;

public sealed class OllamaAssistantTextGenerator : IAssistantTextGenerator
{
    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;
    private readonly IAiProfilePreferenceService _profilePreferenceService;

    public OllamaAssistantTextGenerator(HttpClient httpClient, IOptions<AiOptions> options, IAiProfilePreferenceService profilePreferenceService)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _profilePreferenceService = profilePreferenceService;
    }

    public async Task<string?> GenerateReplyAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || !string.Equals(_options.Provider, "ollama", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var settings = _profilePreferenceService.GetCurrent();

        var request = new OllamaGenerateRequest(
            Model: settings.ActiveModel,
            Prompt: $"{systemPrompt}\n\nUser message: {userPrompt}",
            Stream: false);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/generate")
        {
            Content = JsonContent.Create(request)
        };

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken: cancellationToken);
        return payload?.Response?.Trim();
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

        var request = new OllamaGenerateRequest(
            Model: settings.ActiveModel,
            Prompt: $"{systemPrompt}\n\nUser message: {userPrompt}",
            Stream: true);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/generate")
        {
            Content = JsonContent.Create(request)
        };

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        using var response = await _httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
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

            if (!string.IsNullOrEmpty(chunk?.Response))
            {
                yield return chunk.Response;
            }

            if (chunk?.Done == true)
            {
                yield break;
            }
        }
    }

    private sealed record OllamaGenerateRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("stream")] bool Stream);

    private sealed record OllamaGenerateResponse(
        [property: JsonPropertyName("response")] string? Response);

    private sealed record OllamaStreamChunk(
        [property: JsonPropertyName("response")] string? Response,
        [property: JsonPropertyName("done")] bool Done);
}
