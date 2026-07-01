using FluentAssertions;
using Application.Features.Chat;

namespace Application.Tests;

public class ChatValidatorTests
{
    [Fact]
    public void SendMessageRequestValidator_Should_pass_for_valid_text_message()
    {
        var validator = new SendMessageRequestValidator();
        var request = new SendMessageRequest
        {
            ConversationId = 1,
            MessageType = "Text",
            Content = "Hello"
        };
        var result = validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void SendMessageRequestValidator_Should_fail_when_conversationId_is_zero()
    {
        var validator = new SendMessageRequestValidator();
        var request = new SendMessageRequest
        {
            ConversationId = 0,
            MessageType = "Text",
            Content = "Hello"
        };
        var result = validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConversationId");
    }

    [Fact]
    public void SendMessageRequestValidator_Should_fail_for_invalid_message_type()
    {
        var validator = new SendMessageRequestValidator();
        var request = new SendMessageRequest
        {
            ConversationId = 1,
            MessageType = "InvalidType",
            Content = "Hello"
        };
        var result = validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MessageType");
    }

    [Fact]
    public void CreateConversationRequestValidator_Should_pass_for_valid_request()
    {
        var validator = new CreateConversationRequestValidator();
        var request = new CreateConversationRequest { ParticipantUserId = 5 };
        var result = validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateConversationRequestValidator_Should_fail_when_participantId_is_zero()
    {
        var validator = new CreateConversationRequestValidator();
        var request = new CreateConversationRequest { ParticipantUserId = 0 };
        var result = validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void EditMessageRequestValidator_Should_fail_for_empty_content()
    {
        var validator = new EditMessageRequestValidator();
        var request = new EditMessageRequest { Content = "" };
        var result = validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }
}
