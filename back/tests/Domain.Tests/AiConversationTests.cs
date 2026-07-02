using Domain.Entities;
using Domain.Enums;
using FluentAssertions;

namespace Domain.Tests;

public class AiConversationTests
{
    private static AiConversation CreateConversation() => new()
    {
        UserId = 1,
        UserRole = "Customer",
        Title = "Test",
        Language = "en"
    };

    public class OwnerShip
    {
        [Fact]
        public void Should_be_owned_by_user_when_userId_set()
        {
            var conv = CreateConversation();
            conv.UserId.Should().Be(1);
            conv.SessionId.Should().BeNull();
        }

        [Fact]
        public void Should_be_owned_by_session_when_sessionId_set()
        {
            var conv = new AiConversation
            {
                SessionId = "sess-abc",
                UserRole = "Guest"
            };
            conv.UserId.Should().BeNull();
            conv.SessionId.Should().Be("sess-abc");
        }

        [Fact]
        public void Default_role_should_be_Guest()
        {
            var conv = new AiConversation();
            conv.UserRole.Should().Be("Guest");
        }
    }

    public class SoftDelete
    {
        [Fact]
        public void Should_mark_deleted()
        {
            var conv = CreateConversation();
            conv.IsDeleted = true;
            conv.DeletedAt = DateTime.UtcNow;
            conv.IsDeleted.Should().BeTrue();
            conv.DeletedAt.Should().NotBeNull();
        }
    }

    public class Archive
    {
        [Fact]
        public void Should_support_archive_flag()
        {
            var conv = CreateConversation();
            conv.IsArchived.Should().BeFalse();
            conv.IsArchived = true;
            conv.IsArchived.Should().BeTrue();
        }

        [Fact]
        public void Should_support_hidden_flag()
        {
            var conv = CreateConversation();
            conv.IsHidden.Should().BeFalse();
            conv.IsHidden = true;
            conv.IsHidden.Should().BeTrue();
        }
    }

    public class Messages
    {
        [Fact]
        public void Should_start_with_empty_messages()
        {
            var conv = CreateConversation();
            conv.Messages.Should().BeEmpty();
        }

        [Fact]
        public void Should_allow_adding_messages()
        {
            var conv = CreateConversation();
            var msg = new AiMessage
            {
                ConversationId = conv.Id,
                Role = AiMessageRole.User,
                Content = "Hello"
            };
            conv.Messages.Add(msg);
            conv.Messages.Should().ContainSingle();
        }
    }
}
