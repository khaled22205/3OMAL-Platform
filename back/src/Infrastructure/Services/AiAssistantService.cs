using System.Text.Json;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Features.AiAssistant;
using Infrastructure.Configuration;
using Infrastructure.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AiAssistantService : IAiAssistantService
{
    private readonly IAiConversationService _conversationService;
    private readonly IAiProvider _aiProvider;
    private readonly IKnowledgeService _knowledgeService;
    private readonly AiContextBuilder _contextBuilder;
    private readonly ILogger<AiAssistantService> _logger;
    private readonly AiAssistantOptions _options;
    private readonly AppDbContext _context;

    public AiAssistantService(
        IAiConversationService conversationService,
        IAiProvider aiProvider,
        IKnowledgeService knowledgeService,
        AiContextBuilder contextBuilder,
        IOptions<AiAssistantOptions> options,
        ILogger<AiAssistantService> logger,
        AppDbContext context)
    {
        _conversationService = conversationService;
        _aiProvider = aiProvider;
        _knowledgeService = knowledgeService;
        _contextBuilder = contextBuilder;
        _options = options.Value;
        _logger = logger;
        _context = context;
    }

    public async Task<AiConversationSummaryResponse> StartConversationAsync(int userId, string userRole, StartConversationRequest request)
    {
        var language = DetectLanguage(request.FirstMessage ?? request.Title ?? "");
        return await _conversationService.CreateConversationAsync(userId, language, request.Title, request.FirstMessage);
    }

    public async Task<AiMessageResponse> SendMessageAsync(int userId, string userRole, SendAiMessageRequest request)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var conversation = await _conversationService.GetConversationAsync(request.ConversationId, userId);
        if (conversation == null)
            throw new KeyNotFoundException("Conversation not found");

        var language = conversation.Language;

        await _conversationService.AddMessageAsync(request.ConversationId, userId, "User", request.Content);

        var retrievalStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var knowledge = await _knowledgeService.RetrieveAsync(request.Content, new KnowledgeContext
        {
            Language = language,
            UserId = userId,
            Roles = [userRole]
        }, _options.MaxRetrievalResults);
        retrievalStopwatch.Stop();

        var history = conversation.Messages
            .TakeLast(_options.MaxContextMessages)
            .Select(m => new AiHistoryMessage { Role = m.Role, Content = m.Content })
            .ToList();

        var prompt = _contextBuilder.Build(new AiRequestContext
        {
            UserMessage = request.Content,
            UserRole = userRole,
            Language = language,
            KnowledgeContext = knowledge,
            History = history.Select(h => new HistoryEntry { Role = h.Role, Content = h.Content }).ToList()
        });

        var geminiStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var aiResponse = await _aiProvider.GenerateAsync(new AiRequest
        {
            SystemPrompt = prompt,
            UserMessage = request.Content,
            History = history,
            Temperature = _options.Temperature,
            MaxTokens = _options.MaxTokens
        });
        geminiStopwatch.Stop();

        stopwatch.Stop();

        var response = await _conversationService.AddMessageAsync(
            request.ConversationId, userId, "Assistant", aiResponse.Content, knowledge);

        _logger.LogInformation(
            "AI request completed: conversation={ConvId}, user={UserId}, role={Role}, " +
            "retrieval={RetrievalMs}ms, gemini={GeminiMs}ms, total={TotalMs}ms, " +
            "promptTokens={PromptTokens}, responseTokens={ResponseTokens}",
            request.ConversationId, userId, userRole,
            retrievalStopwatch.ElapsedMilliseconds, geminiStopwatch.ElapsedMilliseconds,
            stopwatch.ElapsedMilliseconds, aiResponse.PromptTokens, aiResponse.ResponseTokens);

        await LogUsageAsync(userId, userRole, aiResponse.PromptTokens, aiResponse.ResponseTokens,
            retrievalStopwatch.ElapsedMilliseconds, stopwatch.ElapsedMilliseconds, false, null);

        return response;
    }

    public async IAsyncEnumerable<AiStreamChunkResponse> SendMessageStreamAsync(
        int userId, string userRole, SendAiMessageRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationService.GetConversationAsync(request.ConversationId, userId);
        if (conversation == null)
        {
            yield return new AiStreamChunkResponse
            {
                ConversationId = request.ConversationId,
                IsComplete = true,
                Error = "Conversation not found"
            };
            yield break;
        }

        var language = conversation.Language;

        await _conversationService.AddMessageAsync(request.ConversationId, userId, "User", request.Content);

        var knowledge = await _knowledgeService.RetrieveAsync(request.Content, new KnowledgeContext
        {
            Language = language,
            UserId = userId,
            Roles = [userRole]
        }, _options.MaxRetrievalResults);

        var history = conversation.Messages
            .TakeLast(_options.MaxContextMessages)
            .Select(m => new AiHistoryMessage { Role = m.Role, Content = m.Content })
            .ToList();

        var prompt = _contextBuilder.Build(new AiRequestContext
        {
            UserMessage = request.Content,
            UserRole = userRole,
            Language = language,
            KnowledgeContext = knowledge,
            History = history.Select(h => new HistoryEntry { Role = h.Role, Content = h.Content }).ToList()
        });

        var fullContent = new System.Text.StringBuilder();

        await foreach (var chunk in _aiProvider.GenerateStreamAsync(new AiRequest
        {
            SystemPrompt = prompt,
            UserMessage = request.Content,
            History = history,
            Temperature = _options.Temperature,
            MaxTokens = _options.MaxTokens
        }, cancellationToken))
        {
            fullContent.Append(chunk);
            yield return new AiStreamChunkResponse
            {
                ConversationId = request.ConversationId,
                Content = chunk,
                IsComplete = false
            };
        }

        var response = await _conversationService.AddMessageAsync(
            request.ConversationId, userId, "Assistant", fullContent.ToString(), knowledge);

        yield return new AiStreamChunkResponse
        {
            ConversationId = request.ConversationId,
            MessageId = response.Id,
            IsComplete = true,
            Sources = response.Sources
        };
    }

    public Task<AiSuggestedPromptsResponse> GetSuggestedPromptsAsync(string userRole)
    {
        var prompts = userRole switch
        {
            "Admin" => new List<string>
            {
                "Show me platform statistics",
                "How many active bookings?",
                "Total revenue this month",
                "Top categories by bookings",
                "Number of registered workers"
            },
            "Worker" => new List<string>
            {
                "Show my upcoming bookings",
                "How do I update my services?",
                "Platform commission rate",
                "How do reviews work?",
                "Tips for getting more bookings"
            },
            "Customer" => new List<string>
            {
                "Recommend a plumber near me",
                "How do I book a service?",
                "Show my booking history",
                "How does payment work?",
                "What categories are available?"
            },
            _ => new List<string>
            {
                "What services does 3OMAL offer?",
                "How do I register as a customer?",
                "How do I become a worker?",
                "What is the booking process?",
                "How does pricing work?"
            }
        };

        return Task.FromResult(new AiSuggestedPromptsResponse { Prompts = prompts });
    }

    private static string DetectLanguage(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "en";

        var arabicCount = text.Count(c => c is >= '\u0600' and <= '\u06FF' or >= '\u0750' and <= '\u077F' or >= '\u08A0' and <= '\u08FF');
        return arabicCount > text.Length / 3 ? "ar" : "en";
    }

    private async Task LogUsageAsync(int? userId, string? role, int promptTokens, int responseTokens,
        long retrievalMs, long totalMs, bool isError, string? errorMessage, string? model = null)
    {
        try
        {
            _context.AiUsageLogs.Add(new AiUsageLog
            {
                UserId = userId,
                Role = role,
                PromptTokens = promptTokens,
                ResponseTokens = responseTokens,
                RetrievalDurationMs = (int)retrievalMs,
                TotalDurationMs = (int)totalMs,
                Model = model ?? "gemini",
                IsError = isError,
                ErrorMessage = errorMessage,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log AI usage");
        }
    }
}
