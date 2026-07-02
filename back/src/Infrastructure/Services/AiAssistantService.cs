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

    public async Task<AiConversationSummaryResponse> StartConversationAsync(
        int? userId, string? sessionId, string userRole, StartConversationRequest request)
    {
        var language = DetectLanguage(request.FirstMessage ?? request.Title ?? "");
        return await _conversationService.CreateConversationAsync(
            userId, sessionId, userRole, language, request.Title, request.FirstMessage);
    }

    public async Task<AiMessageResponse> SendMessageAsync(
        int? userId, string? sessionId, string userRole, SendAiMessageRequest request)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var conversation = await _conversationService.GetConversationAsync(
            request.ConversationId, userId, sessionId, userRole);
        if (conversation == null)
            throw new KeyNotFoundException("Conversation not found");

        var language = conversation.Language;

        await _conversationService.AddMessageAsync(
            request.ConversationId, userId, sessionId, userRole, "User", request.Content);

        var retrievalStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var knowledge = await _knowledgeService.RetrieveAsync(request.Content, new KnowledgeContext
        {
            Language = language,
            UserId = userId,
            Roles = [userRole]
        }, _options.MaxRetrievalResults);
        retrievalStopwatch.Stop();

        // Role-specific live data injection
        var roleData = await BuildRoleContextAsync(userId, userRole);
        if (roleData.Count > 0)
            knowledge = [.. roleData, .. knowledge];

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
            request.ConversationId, userId, sessionId, userRole, "Assistant", aiResponse.Content, knowledge);

        _logger.LogInformation(
            "AI request: conversation={ConvId}, user={UserId}, session={SessionId}, role={Role}, " +
            "retrieval={RetrievalMs}ms, gemini={GeminiMs}ms, total={TotalMs}ms",
            request.ConversationId, userId, sessionId, userRole,
            retrievalStopwatch.ElapsedMilliseconds, geminiStopwatch.ElapsedMilliseconds,
            stopwatch.ElapsedMilliseconds);

        await LogUsageAsync(userId, userRole, aiResponse.PromptTokens, aiResponse.ResponseTokens,
            retrievalStopwatch.ElapsedMilliseconds, stopwatch.ElapsedMilliseconds, false, null);

        return response;
    }

    public async IAsyncEnumerable<AiStreamChunkResponse> SendMessageStreamAsync(
        int? userId, string? sessionId, string userRole, SendAiMessageRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationService.GetConversationAsync(
            request.ConversationId, userId, sessionId, userRole);
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

        await _conversationService.AddMessageAsync(
            request.ConversationId, userId, sessionId, userRole, "User", request.Content);

        var knowledge = await _knowledgeService.RetrieveAsync(request.Content, new KnowledgeContext
        {
            Language = language,
            UserId = userId,
            Roles = [userRole]
        }, _options.MaxRetrievalResults);

        // Role-specific live data injection
        var roleData = await BuildRoleContextAsync(userId, userRole);
        if (roleData.Count > 0)
            knowledge = [.. roleData, .. knowledge];

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
            request.ConversationId, userId, sessionId, userRole,
            "Assistant", fullContent.ToString(), knowledge);

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
                "كام مستخدم عندنا على المنصة؟",
                "كام حجز اتعمل النهارده؟",
                "وريني إجمالي الإيرادات الشهر ده",
                "Top categories by bookings",
                "Number of registered workers",
                "What is the platform commission rate?"
            },
            "Worker" => new List<string>
            {
                "وريني حجوزاتي الجاية",
                "إزاي أحدّث خدماتي؟",
                "Platform commission rate",
                "Reviews system explained",
                "Tips to get more bookings"
            },
            "Customer" => new List<string>
            {
                "محتاج سباك كويس في منطقتي",
                "إزاي أعمل حجز؟",
                "وريني سجل حجوزاتي",
                "How does payment work?",
                "Recommend an electrician",
                "Recommend an AC technician"
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

    // --- Permission-Aware Role Context Builders ---

    private static string DetectLanguage(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "en";
        var arabicCount = text.Count(c =>
            c is >= '\u0600' and <= '\u06FF' or >= '\u0750' and <= '\u077F' or >= '\u08A0' and <= '\u08FF');
        return arabicCount > text.Length / 3 ? "ar" : "en";
    }

    private async Task<List<SearchResult>> BuildRoleContextAsync(int? userId, string userRole)
    {
        if (userId == null) return [];

        return userRole switch
        {
            "Admin" => await BuildAdminStatsContextAsync(),
            "Worker" => await BuildWorkerStatsContextAsync(userId.Value),
            "Customer" => await BuildCustomerStatsContextAsync(userId.Value),
            _ => []
        };
    }

    private async Task<List<SearchResult>> BuildAdminStatsContextAsync()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalWorkers = await _context.WorkerProfiles.Where(w => !w.IsDeleted).CountAsync();
        var totalCustomers = totalUsers - totalWorkers - 1; // approximate (minus admin)
        var totalBookings = await _context.Bookings.Where(b => !b.IsDeleted).CountAsync();
        var activeBookings = await _context.Bookings
            .Where(b => !b.IsDeleted && b.Status != Domain.Enums.BookingStatus.Completed
                && b.Status != Domain.Enums.BookingStatus.Cancelled
                && b.Status != Domain.Enums.BookingStatus.Rejected
                && b.Status != Domain.Enums.BookingStatus.Expired)
            .CountAsync();
        var completedBookings = await _context.Bookings
            .Where(b => !b.IsDeleted && b.Status == Domain.Enums.BookingStatus.Completed)
            .CountAsync();
        var totalRevenue = await _context.Bookings
            .Where(b => !b.IsDeleted && b.Status == Domain.Enums.BookingStatus.Completed)
            .SumAsync(b => b.TotalPrice);
        var totalCommission = await _context.Bookings
            .Where(b => !b.IsDeleted && b.Status == Domain.Enums.BookingStatus.Completed)
            .SumAsync(b => b.CommissionAmount);
        var pendingWorkers = await _context.WorkerProfiles
            .Where(w => !w.IsDeleted && !w.IsVerified)
            .CountAsync();

        return
        [
            new SearchResult
            {
                SourceType = "admin_stats",
                SourceId = 0,
                Title = "Platform Statistics",
                Excerpt = $"Total Users: {totalUsers} | Total Workers: {totalWorkers} | Total Customers: {totalCustomers} | " +
                          $"Total Bookings: {totalBookings} | Active Bookings: {activeBookings} | " +
                          $"Completed Bookings: {completedBookings} | Total Revenue: {totalRevenue:N0} EGP | " +
                          $"Total Commission: {totalCommission:N0} EGP | Pending Worker Verifications: {pendingWorkers}",
                RelevanceScore = 1.0
            }
        ];
    }

    private async Task<List<SearchResult>> BuildWorkerStatsContextAsync(int userId)
    {
        var worker = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);
        if (worker == null) return [];

        var pendingBookings = await _context.Bookings
            .Where(b => b.WorkerProfileId == worker.Id && !b.IsDeleted
                && b.Status == Domain.Enums.BookingStatus.Pending)
            .CountAsync();
        var activeBookings = await _context.Bookings
            .Where(b => b.WorkerProfileId == worker.Id && !b.IsDeleted
                && b.Status != Domain.Enums.BookingStatus.Completed
                && b.Status != Domain.Enums.BookingStatus.Cancelled
                && b.Status != Domain.Enums.BookingStatus.Rejected
                && b.Status != Domain.Enums.BookingStatus.Expired)
            .CountAsync();
        var completedJobs = worker.CompletedJobs;
        var avgRating = worker.AverageRating;
        var totalEarnings = await _context.Bookings
            .Where(b => b.WorkerProfileId == worker.Id && !b.IsDeleted
                && b.Status == Domain.Enums.BookingStatus.Completed)
            .SumAsync(b => b.TotalPrice);

        return
        [
            new SearchResult
            {
                SourceType = "worker_stats",
                SourceId = worker.Id,
                Title = "My Worker Stats",
                Excerpt = $"Pending requests: {pendingBookings} | Active jobs: {activeBookings} | " +
                          $"Completed jobs: {completedJobs} | Rating: {avgRating:F1}/5 | " +
                          $"Total earnings: {totalEarnings:N0} EGP",
                RelevanceScore = 1.0
            }
        ];
    }

    private async Task<List<SearchResult>> BuildCustomerStatsContextAsync(int userId)
    {
        var myBookings = await _context.Bookings
            .Where(b => b.CustomerId == userId && !b.IsDeleted)
            .OrderByDescending(b => b.CreatedAt)
            .Take(5)
            .Select(b => new
            {
                b.Id,
                b.Status,
                b.TotalPrice,
                b.ScheduledAt,
                WorkerName = b.WorkerProfile.UserId.ToString()
            })
            .ToListAsync();

        if (myBookings.Count == 0) return [];

        var bookingSummary = string.Join(" | ",
            myBookings.Select(b => $"#{b.Id}: {b.Status} on {b.ScheduledAt:yyyy-MM-dd} - {b.TotalPrice} EGP"));

        return
        [
            new SearchResult
            {
                SourceType = "customer_stats",
                SourceId = 0,
                Title = "My Recent Bookings",
                Excerpt = $"Your recent bookings ({myBookings.Count} total shown): {bookingSummary}",
                RelevanceScore = 1.0
            }
        ];
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
