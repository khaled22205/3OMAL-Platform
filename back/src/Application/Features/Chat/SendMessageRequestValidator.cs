using FluentValidation;

namespace Application.Features.Chat;

public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.ConversationId).GreaterThan(0);
        RuleFor(x => x.MessageType)
            .NotEmpty()
            .Must(t => new[] { "Text", "Image", "File", "Video", "Emoji", "Hyperlink", "Location" }.Contains(t))
            .WithMessage("Invalid message type");
        RuleFor(x => x.Content)
            .NotEmpty()
            .When(x => x.MessageType is "Text" or "Emoji" or "Hyperlink");
    }
}

public class CreateConversationRequestValidator : AbstractValidator<CreateConversationRequest>
{
    public CreateConversationRequestValidator()
    {
        RuleFor(x => x.ParticipantUserId).GreaterThan(0);
    }
}

public class EditMessageRequestValidator : AbstractValidator<EditMessageRequest>
{
    public EditMessageRequestValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(5000);
    }
}
