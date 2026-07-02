using Domain.Entities;
using Domain.Enums;
using FluentAssertions;

namespace Domain.Tests;

public class AiMessageTests
{
    private static AiMessage CreateUserMessage() => new()
    {
        ConversationId = 1,
        Role = AiMessageRole.User,
        Content = "What services do you offer?"
    };

    private static AiMessage CreateAssistantMessage() => new()
    {
        ConversationId = 1,
        Role = AiMessageRole.Assistant,
        Content = "We offer plumbing, electrical, and carpentry services.",
        PromptTokens = 100,
        ResponseTokens = 50
    };

    public class Role
    {
        [Fact]
        public void Should_identify_user_message()
        {
            var msg = CreateUserMessage();
            msg.Role.Should().Be(AiMessageRole.User);
        }

        [Fact]
        public void Should_identify_assistant_message()
        {
            var msg = CreateAssistantMessage();
            msg.Role.Should().Be(AiMessageRole.Assistant);
        }

        [Fact]
        public void Should_identify_system_message()
        {
            var msg = new AiMessage { Role = AiMessageRole.System, Content = "System prompt" };
            msg.Role.Should().Be(AiMessageRole.System);
        }
    }

    public class Content
    {
        [Fact]
        public void Should_store_message_content()
        {
            var msg = CreateUserMessage();
            msg.Content.Should().Be("What services do you offer?");
        }

        [Fact]
        public void Should_allow_empty_content()
        {
            var msg = new AiMessage { Role = AiMessageRole.Assistant, Content = "" };
            msg.Content.Should().BeEmpty();
        }
    }

    public class TokenTracking
    {
        [Fact]
        public void Should_track_prompt_and_response_tokens()
        {
            var msg = CreateAssistantMessage();
            msg.PromptTokens.Should().Be(100);
            msg.ResponseTokens.Should().Be(50);
        }

        [Fact]
        public void Should_allow_null_tokens()
        {
            var msg = CreateUserMessage();
            msg.PromptTokens.Should().BeNull();
            msg.ResponseTokens.Should().BeNull();
        }
    }

    public class SourceReferences
    {
        [Fact]
        public void Should_store_sources_json()
        {
            var json = """[{"sourceType":"worker","sourceId":1,"title":"Ahmed"}]""";
            var msg = new AiMessage { Role = AiMessageRole.Assistant, Content = "Answer", SourcesJson = json };
            msg.SourcesJson.Should().Be(json);
        }

        [Fact]
        public void Should_allow_null_sources()
        {
            var msg = CreateUserMessage();
            msg.SourcesJson.Should().BeNull();
        }
    }

    public class SoftDelete
    {
        [Fact]
        public void Should_mark_deleted()
        {
            var msg = CreateUserMessage();
            msg.IsDeleted = true;
            msg.DeletedAt = DateTime.UtcNow;
            msg.IsDeleted.Should().BeTrue();
            msg.DeletedAt.Should().NotBeNull();
        }
    }

    public class Relationship
    {
        [Fact]
        public void Should_belong_to_conversation()
        {
            var msg = CreateUserMessage();
            msg.ConversationId.Should().Be(1);
        }
    }
}
