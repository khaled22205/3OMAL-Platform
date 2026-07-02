using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using FluentAssertions;
using Application.Features.AiAssistant;
using Infrastructure.Configuration;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests;

public class AiAssistantServiceTests : IDisposable
{
    private readonly Mock<IAiConversationService> _convServiceMock = new();
    private readonly Mock<IAiProvider> _aiProviderMock = new();
    private readonly Mock<IKnowledgeService> _knowledgeMock = new();
    private readonly AiContextBuilder _contextBuilder = new();
    private readonly Mock<ILogger<AiAssistantService>> _loggerMock = new();
    private readonly AppDbContext _context;
    private readonly AiAssistantOptions _options = new()
    {
        MaxRetrievalResults = 5,
        MaxContextMessages = 10,
        Temperature = 0.7f,
        MaxTokens = 2048
    };

    public AiAssistantServiceTests()
    {
        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(dbOptions);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private AiAssistantService CreateService()
    {
        return new AiAssistantService(
            _convServiceMock.Object,
            _aiProviderMock.Object,
            _knowledgeMock.Object,
            _contextBuilder,
            Options.Create(_options),
            _loggerMock.Object,
            _context);
    }

    [Fact]
    public async Task StartConversationAsync_Should_detect_arabic_language()
    {
        _convServiceMock.Setup(x => x.CreateConversationAsync(
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(new AiConversationSummaryResponse { Id = 1 });

        var service = CreateService();
        var result = await service.StartConversationAsync(null, "sess-1", "Guest",
            new StartConversationRequest { FirstMessage = "مرحبا كيف الحال" });

        _convServiceMock.Verify(x => x.CreateConversationAsync(
            null, "sess-1", "Guest", "ar", null, "مرحبا كيف الحال"), Times.Once);
    }

    [Fact]
    public async Task StartConversationAsync_Should_detect_english_language()
    {
        _convServiceMock.Setup(x => x.CreateConversationAsync(
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(new AiConversationSummaryResponse { Id = 1 });

        var service = CreateService();
        var result = await service.StartConversationAsync(1, null, "Customer",
            new StartConversationRequest { FirstMessage = "Hello, I need help" });

        _convServiceMock.Verify(x => x.CreateConversationAsync(
            1, null, "Customer", "en", null, "Hello, I need help"), Times.Once);
    }

    [Fact]
    public async Task StartConversationAsync_Should_return_conversation()
    {
        _convServiceMock.Setup(x => x.CreateConversationAsync(
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(new AiConversationSummaryResponse { Id = 5, Title = "Test" });

        var service = CreateService();
        var result = await service.StartConversationAsync(1, null, "Customer",
            new StartConversationRequest { Title = "Test", FirstMessage = "Hi" });

        result.Id.Should().Be(5);
    }

    [Fact]
    public async Task SendMessageAsync_Should_throw_when_conversation_not_found()
    {
        _convServiceMock.Setup(x => x.GetConversationAsync(
                It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>()))
            .ReturnsAsync((AiConversationDetailResponse?)null);

        var service = CreateService();
        var act = () => service.SendMessageAsync(1, null, "Customer",
            new SendAiMessageRequest { ConversationId = 999, Content = "Hey" });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task SendMessageAsync_Should_query_knowledge()
    {
        _convServiceMock.Setup(x => x.GetConversationAsync(
                It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>()))
            .ReturnsAsync(new AiConversationDetailResponse { Language = "en", Messages = [] });
        _convServiceMock.Setup(x => x.AddMessageAsync(
                It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<SearchResult>?>()))
            .ReturnsAsync(new AiMessageResponse());
        _aiProviderMock.Setup(x => x.GenerateAsync(It.IsAny<AiRequest>()))
            .ReturnsAsync(new AiResponse { Content = "Response", PromptTokens = 10, ResponseTokens = 20 });
        _knowledgeMock.Setup(x => x.RetrieveAsync(It.IsAny<string>(), It.IsAny<KnowledgeContext>(), It.IsAny<int>()))
            .ReturnsAsync(new List<SearchResult>());

        var service = CreateService();
        await service.SendMessageAsync(1, null, "Customer",
            new SendAiMessageRequest { ConversationId = 1, Content = "Tell me about plumbing" });

        _knowledgeMock.Verify(x => x.RetrieveAsync("Tell me about plumbing",
            It.Is<KnowledgeContext>(kc => kc.UserId == 1 && kc.Roles.Contains("Customer")),
            5), Times.Once);
    }

    [Fact]
    public async Task SendMessageStreamAsync_Should_stream_chunks()
    {
        _convServiceMock.Setup(x => x.GetConversationAsync(
                It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>()))
            .ReturnsAsync(new AiConversationDetailResponse { Language = "en", Messages = [] });
        _convServiceMock.Setup(x => x.AddMessageAsync(
                It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<SearchResult>?>()))
            .ReturnsAsync(new AiMessageResponse { Id = 42 });
        _knowledgeMock.Setup(x => x.RetrieveAsync(It.IsAny<string>(), It.IsAny<KnowledgeContext>(), It.IsAny<int>()))
            .ReturnsAsync(new List<SearchResult>());
        _aiProviderMock.Setup(x => x.GenerateStreamAsync(It.IsAny<AiRequest>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable("Chunk1", "Chunk2"));

        var service = CreateService();
        var chunks = new List<AiStreamChunkResponse>();
        await foreach (var chunk in service.SendMessageStreamAsync(1, null, "Customer",
            new SendAiMessageRequest { ConversationId = 1, Content = "Hello" }))
        {
            chunks.Add(chunk);
        }

        chunks.Should().HaveCount(3); // 2 content chunks + 1 complete
        chunks[0].Content.Should().Be("Chunk1");
        chunks[1].Content.Should().Be("Chunk2");
        chunks[2].IsComplete.Should().BeTrue();
        chunks[2].MessageId.Should().Be(42);
    }

    [Fact]
    public async Task SendMessageStreamAsync_Should_return_error_when_conversation_not_found()
    {
        _convServiceMock.Setup(x => x.GetConversationAsync(
                It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>()))
            .ReturnsAsync((AiConversationDetailResponse?)null);

        var service = CreateService();
        var chunks = new List<AiStreamChunkResponse>();
        await foreach (var chunk in service.SendMessageStreamAsync(1, null, "Customer",
            new SendAiMessageRequest { ConversationId = 999, Content = "Hi" }))
        {
            chunks.Add(chunk);
        }

        chunks.Should().ContainSingle();
        chunks[0].Error.Should().Be("Conversation not found");
        chunks[0].IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task GetSuggestedPromptsAsync_Should_return_role_specific_prompts()
    {
        var service = CreateService();
        var adminResult = await service.GetSuggestedPromptsAsync("Admin");
        var guestResult = await service.GetSuggestedPromptsAsync("Guest");

        adminResult.Prompts.Should().Contain(p => p.Contains("إيرادات"));
        guestResult.Prompts.Should().Contain(p => p.Contains("register"));
    }

    /// <summary>Helper to create an async enumerable from params.</summary>
    private static async IAsyncEnumerable<T> AsyncEnumerable<T>(params T[] items)
    {
        foreach (var item in items)
        {
            yield return item;
            await Task.CompletedTask;
        }
    }
}
