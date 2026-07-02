using Infrastructure.Data;
using Infrastructure.Services;
using Application.Features.AiAssistant;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Infrastructure.Tests;

public class AiConversationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AiConversationService _service;
    private readonly Mock<ILogger<AiConversationService>> _loggerMock = new();

    public AiConversationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new AiConversationService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateConversationAsync_Should_assign_userId_for_authenticated_user()
    {
        var result = await _service.CreateConversationAsync(1, null, "Customer", "en", "Test", null);

        result.UserId.Should().Be(1);
        result.SessionId.Should().BeNull();
        result.UserRole.Should().Be("Customer");
        result.Title.Should().Be("Test");
    }

    [Fact]
    public async Task CreateConversationAsync_Should_assign_sessionId_for_guest()
    {
        var result = await _service.CreateConversationAsync(null, "sess-abc", "Guest", "ar", null, "Hello");

        result.UserId.Should().BeNull();
        result.SessionId.Should().Be("sess-abc");
        result.UserRole.Should().Be("Guest");
    }

    [Fact]
    public async Task CreateConversationAsync_Should_store_in_database()
    {
        await _service.CreateConversationAsync(1, null, "Customer", "en", "Title", "First msg");
        var count = await _context.AiConversations.CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task GetConversationsAsync_Should_return_only_user_conversations()
    {
        await _service.CreateConversationAsync(1, null, "Customer", "en", "User1", null);
        await _service.CreateConversationAsync(2, null, "Worker", "en", "User2", null);

        var result = await _service.GetConversationsAsync(1, null, "Customer", 1, 20);
        result.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("User1");
    }

    [Fact]
    public async Task GetConversationsAsync_Should_return_only_session_conversations()
    {
        await _service.CreateConversationAsync(null, "sess-a", "Guest", "en", "SessA", null);
        await _service.CreateConversationAsync(null, "sess-b", "Guest", "en", "SessB", null);

        var result = await _service.GetConversationsAsync(null, "sess-a", "Guest", 1, 20);
        result.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("SessA");
    }

    [Fact]
    public async Task GetConversationAsync_Should_return_null_for_wrong_user()
    {
        var conv = await _service.CreateConversationAsync(1, null, "Customer", "en", "Private", null);
        var result = await _service.GetConversationAsync(conv.Id, 2, null, "Customer");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetConversationAsync_Should_return_null_for_wrong_session()
    {
        var conv = await _service.CreateConversationAsync(null, "sess-a", "Guest", "en", "Private", null);
        var result = await _service.GetConversationAsync(conv.Id, null, "sess-b", "Guest");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetConversationAsync_Should_return_detail_for_owner()
    {
        var conv = await _service.CreateConversationAsync(1, null, "Customer", "en", "My conv", "First msg");
        var result = await _service.GetConversationAsync(conv.Id, 1, null, "Customer");
        result.Should().NotBeNull();
        result!.Title.Should().Be("My conv");
    }

    [Fact]
    public async Task DeleteConversationAsync_Should_soft_delete()
    {
        var conv = await _service.CreateConversationAsync(1, null, "Customer", "en", "To delete", null);
        var deleted = await _service.DeleteConversationAsync(conv.Id, 1, null, "Customer");
        deleted.Should().BeTrue();

        var fromDb = await _context.AiConversations.IgnoreQueryFilters().FirstAsync(c => c.Id == conv.Id);
        fromDb.IsDeleted.Should().BeTrue();
        fromDb.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteConversationAsync_Should_return_false_for_non_owner()
    {
        var conv = await _service.CreateConversationAsync(1, null, "Customer", "en", "Mine", null);
        var deleted = await _service.DeleteConversationAsync(conv.Id, 2, null, "Customer");
        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task SearchConversationsAsync_Should_filter_by_query()
    {
        await _service.CreateConversationAsync(1, null, "Customer", "en", "Plumbing help", null);
        await _service.CreateConversationAsync(1, null, "Customer", "en", "Electrical issue", null);

        var result = await _service.SearchConversationsAsync(1, null, "Customer", "Plumb", 1, 20);
        result.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("Plumbing help");
    }

    [Fact]
    public async Task AddMessageAsync_Should_store_message()
    {
        var conv = await _service.CreateConversationAsync(1, null, "Customer", "en", "Conv", null);
        var msg = await _service.AddMessageAsync(conv.Id, 1, null, "Customer", "User", "Hello AI");

        msg.Role.Should().Be("User");
        msg.Content.Should().Be("Hello AI");

        var count = await _context.AiMessages.CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task AddMessageAsync_Should_store_sources()
    {
        var conv = await _service.CreateConversationAsync(1, null, "Customer", "en", "Conv", null);
        var sources = new List<SearchResult>
        {
            new() { SourceType = "category", SourceId = 1, Title = "Plumbing", RelevanceScore = 0.9 }
        };
        var msg = await _service.AddMessageAsync(conv.Id, 1, null, "Customer", "Assistant", "Answer", sources);
        msg.Sources.Should().ContainSingle();
        msg.Sources[0].Title.Should().Be("Plumbing");
    }

    [Fact]
    public async Task AddMessageAsync_Should_return_message_even_for_deleted_conversation()
    {
        var msg = await _service.AddMessageAsync(999, 1, null, "Customer", "User", "msg");
        msg.Should().NotBeNull();
        msg.Content.Should().Be("msg");
    }

    [Fact]
    public async Task GetMessageCountAsync_Should_return_correct_count()
    {
        var conv = await _service.CreateConversationAsync(1, null, "Customer", "en", "Conv", null);
        await _service.AddMessageAsync(conv.Id, 1, null, "Customer", "User", "First msg");
        var count = await _service.GetMessageCountAsync(conv.Id);
        count.Should().Be(1);
    }
}
