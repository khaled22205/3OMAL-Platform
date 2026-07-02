using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Features.AiAssistant;
using Infrastructure.Configuration;

namespace Infrastructure.Services;

public class GeminiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiProvider> _logger;

    public string ProviderName => "Gemini";

    public GeminiProvider(HttpClient httpClient, IOptions<GeminiOptions> options, ILogger<GeminiProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AiResponse> GenerateAsync(AiRequest request, CancellationToken cancellationToken = default)
    {
        var model = _options.Model;
        var payload = BuildPayload(request);

        for (int attempt = 0; attempt <= _options.RetryCount; attempt++)
        {
            try
            {
                var url = $"{_options.Endpoint}/models/{model}:generateContent?key={_options.ApiKey}";
                var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken);
                var content = ExtractContent(result);

                return new AiResponse
                {
                    Content = content,
                    PromptTokens = result?.UsageMetadata?.PromptTokenCount ?? 0,
                    ResponseTokens = result?.UsageMetadata?.CandidatesTokenCount ?? 0
                };
            }
            catch (Exception ex) when (attempt < _options.RetryCount)
            {
                _logger.LogWarning(ex, "Gemini API attempt {Attempt} failed for model {Model}, retrying...",
                    attempt + 1, model);

                if (ex.Message.Contains("not found") && model != _options.FallbackModel)
                {
                    model = _options.FallbackModel;
                    _logger.LogInformation("Falling back to model {FallbackModel}", model);
                }

                await Task.Delay(1000 * (attempt + 1), cancellationToken);
            }
        }

        throw new InvalidOperationException("Gemini API failed after all retry attempts");
    }

    public async IAsyncEnumerable<string> GenerateStreamAsync(AiRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var channel = System.Threading.Channels.Channel.CreateUnbounded<string>();
        var writer = channel.Writer;

        var streamingTask = StreamWithRetriesAsync(request, writer, cancellationToken);

        await foreach (var chunk in channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return chunk;
        }

        await streamingTask;
    }

    private async Task StreamWithRetriesAsync(AiRequest request, System.Threading.Channels.ChannelWriter<string> writer, CancellationToken cancellationToken)
    {
        var model = _options.Model;
        var payload = BuildPayload(request);

        for (int attempt = 0; attempt <= _options.RetryCount; attempt++)
        {
            try
            {
                var url = $"{_options.Endpoint}/models/{model}:streamGenerateContent?alt=sse&key={_options.ApiKey}";
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(payload)
                };

                using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var reader = new StreamReader(stream);

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var line = await reader.ReadLineAsync(cancellationToken);
                    if (line == null) break;
                    if (!line.StartsWith("data: ")) continue;

                    var json = line[6..];
                    if (json == "[DONE]") break;

                    var chunk = JsonSerializer.Deserialize<GeminiResponse>(json);
                    var text = ExtractContent(chunk);
                    if (!string.IsNullOrEmpty(text))
                        await writer.WriteAsync(text, cancellationToken);
                }

                writer.Complete();
                return;
            }
            catch (OperationCanceledException)
            {
                writer.TryComplete();
                return;
            }
            catch (Exception ex) when (attempt < _options.RetryCount)
            {
                _logger.LogWarning(ex, "Gemini streaming attempt {Attempt}/{MaxRetries} failed for model {Model}, retrying...",
                    attempt + 1, _options.RetryCount, model);

                if (ex.Message.Contains("not found") && model != _options.FallbackModel)
                {
                    model = _options.FallbackModel;
                    _logger.LogInformation("Falling back to model {FallbackModel}", model);
                }

                await Task.Delay(1000 * (attempt + 1), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini streaming failed after all retry attempts");
                writer.TryComplete(ex);
                return;
            }
        }

        writer.TryComplete(new InvalidOperationException("Gemini streaming failed after all retry attempts"));
    }

    private object BuildPayload(AiRequest request)
    {
        var contents = new List<object>();

        foreach (var msg in request.History)
        {
            var role = msg.Role.Equals("Assistant", StringComparison.OrdinalIgnoreCase) ? "model" : "user";
            contents.Add(new
            {
                role,
                parts = new[] { new { text = msg.Content } }
            });
        }

        contents.Add(new
        {
            role = "user",
            parts = new[] { new { text = $"{request.SystemPrompt}\n\nUSER QUESTION: {request.UserMessage}" } }
        });

        return new
        {
            contents,
            generationConfig = new
            {
                temperature = request.Temperature,
                maxOutputTokens = request.MaxTokens,
                topP = _options.TopP,
                topK = _options.TopK
            },
            safetySettings = new[]
            {
                new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_NONE" },
                new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_NONE" },
                new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
                new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" }
            }
        };
    }

    private static string ExtractContent(GeminiResponse? response)
    {
        if (response?.Candidates == null || response.Candidates.Count == 0)
            return string.Empty;

        var candidate = response.Candidates[0];
        if (candidate.Content?.Parts == null) return string.Empty;

        return string.Concat(candidate.Content.Parts.Select(p => p.Text));
    }
}

public class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate>? Candidates { get; set; }

    [JsonPropertyName("usageMetadata")]
    public GeminiUsageMetadata? UsageMetadata { get; set; }
}

public class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; set; }

    [JsonPropertyName("finishReason")]
    public string? FinishReason { get; set; }
}

public class GeminiContent
{
    [JsonPropertyName("parts")]
    public List<GeminiPart>? Parts { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }
}

public class GeminiPart
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class GeminiUsageMetadata
{
    [JsonPropertyName("promptTokenCount")]
    public int PromptTokenCount { get; set; }

    [JsonPropertyName("candidatesTokenCount")]
    public int CandidatesTokenCount { get; set; }

    [JsonPropertyName("totalTokenCount")]
    public int TotalTokenCount { get; set; }
}
