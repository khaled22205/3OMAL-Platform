using FluentAssertions;
using Domain.Entities;
using Domain.Enums;
using Application.Common.Mappings;
using Application.Features.Chat;

namespace Application.Tests;

public class ChatMappingTests
{
    [Fact]
    public void Message_ToResponse_Should_map_correctly()
    {
        var message = new Message
        {
            Id = 1,
            ConversationId = 10,
            SenderId = 5,
            MessageType = MessageType.Text,
            Content = "Hello",
            CreatedAt = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc),
            IsEdited = false,
            IsDeleted = false
        };

        var response = message.ToResponse("John Doe");

        response.Id.Should().Be(1);
        response.ConversationId.Should().Be(10);
        response.SenderId.Should().Be(5);
        response.SenderName.Should().Be("John Doe");
        response.MessageType.Should().Be("Text");
        response.Content.Should().Be("Hello");
        response.IsEdited.Should().BeFalse();
        response.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Attachment_ToResponse_Should_map_correctly()
    {
        var attachment = new MessageAttachment
        {
            Id = 1,
            MessageId = 10,
            FileName = "photo.jpg",
            FilePath = "/uploads/photo.jpg",
            ContentType = "image/jpeg",
            FileSize = 1024,
            AttachmentType = "Image"
        };

        var response = attachment.ToResponse();

        response.Id.Should().Be(1);
        response.FileName.Should().Be("photo.jpg");
        response.ContentType.Should().Be("image/jpeg");
        response.FileSize.Should().Be(1024);
        response.AttachmentType.Should().Be("Image");
    }

    [Fact]
    public void UserBrief_ToBriefResponse_Should_map_correctly()
    {
        var response = (5, "John", "Doe", "photo.jpg").ToBriefResponse();

        response.UserId.Should().Be(5);
        response.FirstName.Should().Be("John");
        response.LastName.Should().Be("Doe");
        response.Photo.Should().Be("photo.jpg");
    }

    [Fact]
    public void Conversation_ToResponse_Should_map_correctly()
    {
        var conversation = new Conversation
        {
            Id = 1,
            LastMessageContent = "Hello",
            LastMessageAt = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        var otherUser = new UserBriefResponse
        {
            UserId = 10,
            FirstName = "Ahmed",
            LastName = "Saeed",
            Photo = null
        };

        var response = conversation.ToResponse(otherUser, 3);

        response.Id.Should().Be(1);
        response.OtherUser.UserId.Should().Be(10);
        response.OtherUser.FirstName.Should().Be("Ahmed");
        response.UnreadCount.Should().Be(3);
        response.LastMessageAt.Should().NotBeNull();
    }
}
