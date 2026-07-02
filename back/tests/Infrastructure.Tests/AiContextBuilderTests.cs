using Application.Features.AiAssistant;
using FluentAssertions;
using Infrastructure.Services;

namespace Infrastructure.Tests;

public class AiContextBuilderTests
{
    private readonly AiContextBuilder _builder = new();

    private static AiRequestContext CreateContext(string userRole = "Guest", string? language = null)
    {
        return new AiRequestContext
        {
            UserMessage = "What services do you offer?",
            UserRole = userRole,
            Language = language,
            KnowledgeContext = [],
            History = []
        };
    }

    [Fact]
    public void Build_Should_contain_system_prompt()
    {
        var result = _builder.Build(CreateContext());
        result.Should().Contain("3OMAL AI");
        result.Should().Contain("RULES:");
    }

    [Fact]
    public void Build_Should_not_mix_languages_by_default()
    {
        var result = _builder.Build(CreateContext(language: null));
        result.Should().Contain("LANGUAGE: Detect the user's language");
    }

    [Fact]
    public void Build_Should_include_arabic_instruction_when_language_is_ar()
    {
        var ctx = CreateContext(language: "ar");
        var result = _builder.Build(ctx);
        result.Should().Contain("Egyptian Arabic");
        result.Should().Contain("LANGUAGE INSTRUCTION");
    }

    [Fact]
    public void Build_Should_include_english_instruction_when_language_is_en()
    {
        var ctx = CreateContext(language: "en");
        var result = _builder.Build(ctx);
        result.Should().Contain("Respond EXCLUSIVELY in English");
    }

    [Fact]
    public void Build_Should_include_user_role()
    {
        var ctx = CreateContext(userRole: "Admin");
        var result = _builder.Build(ctx);
        result.Should().Contain("CURRENT USER ROLE: Admin");
    }

    [Fact]
    public void Build_Should_include_admin_permissions()
    {
        var ctx = CreateContext(userRole: "Admin");
        var result = _builder.Build(ctx);
        result.Should().Contain("FULL ACCESS");
        result.Should().Contain("Total users");
    }

    [Fact]
    public void Build_Should_include_worker_permissions()
    {
        var ctx = CreateContext(userRole: "Worker");
        var result = _builder.Build(ctx);
        result.Should().Contain("This user is a WORKER");
        result.Should().Contain("CANNOT access: other workers' data");
    }

    [Fact]
    public void Build_Should_include_customer_permissions()
    {
        var ctx = CreateContext(userRole: "Customer");
        var result = _builder.Build(ctx);
        result.Should().Contain("This user is a CUSTOMER");
        result.Should().Contain("CANNOT access: other customers' data");
    }

    [Fact]
    public void Build_Should_include_guest_permissions()
    {
        var ctx = CreateContext(userRole: "Guest");
        var result = _builder.Build(ctx);
        result.Should().Contain("GUEST user");
        result.Should().Contain("Encourage them to register");
    }

    [Fact]
    public void Build_Should_include_knowledge_context_when_provided()
    {
        var ctx = CreateContext();
        ctx.KnowledgeContext =
        [
            new SearchResult { SourceType = "category", Title = "Plumbing", Excerpt = "All plumbing services" }
        ];
        var result = _builder.Build(ctx);
        result.Should().Contain("RELEVANT PLATFORM KNOWLEDGE");
        result.Should().Contain("Plumbing");
        result.Should().Contain("All plumbing services");
    }

    [Fact]
    public void Build_Should_include_conversation_history()
    {
        var ctx = CreateContext();
        ctx.History =
        [
            new HistoryEntry { Role = "User", Content = "Hello" },
            new HistoryEntry { Role = "Assistant", Content = "Hi there!" }
        ];
        var result = _builder.Build(ctx);
        result.Should().Contain("CONVERSATION HISTORY");
        result.Should().Contain("User: Hello");
        result.Should().Contain("Assistant: Hi there!");
    }

    [Fact]
    public void Build_Should_include_user_message_at_end()
    {
        var ctx = CreateContext();
        var result = _builder.Build(ctx);
        result.Should().Contain("USER QUESTION:");
        result.Should().Contain("What services do you offer?");
    }

    [Fact]
    public void Build_Should_not_include_history_when_empty()
    {
        var ctx = CreateContext();
        var result = _builder.Build(ctx);
        result.Should().NotContain("CONVERSATION HISTORY");
    }

    [Fact]
    public void Build_Should_only_include_recent_history()
    {
        var ctx = CreateContext();
        var many = Enumerable.Range(0, 20).Select(i => new HistoryEntry { Role = "User", Content = $"Msg {i}" }).ToList();
        ctx.History = many;
        var result = _builder.Build(ctx);
        result.Should().Contain("Msg 19");
        result.Should().NotContain("Msg 0");
    }

    [Fact]
    public void Build_Should_include_knowledge_only_when_not_empty()
    {
        var ctx = CreateContext();
        ctx.KnowledgeContext = [];
        var result = _builder.Build(ctx);
        result.Should().NotContain("RELEVANT PLATFORM KNOWLEDGE");
    }
}
