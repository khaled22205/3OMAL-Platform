using FluentValidation;

namespace Application.Features.AiAssistant.Validators;

public class SendAiMessageValidator : AbstractValidator<SendAiMessageRequest>
{
    public SendAiMessageValidator()
    {
        RuleFor(x => x.ConversationId).GreaterThan(0);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
    }
}

public class StartConversationValidator : AbstractValidator<StartConversationRequest>
{
    public StartConversationValidator()
    {
        When(x => x.FirstMessage != null, () =>
        {
            RuleFor(x => x.FirstMessage!).NotEmpty().MaximumLength(2000);
        });
        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title!).MaximumLength(200);
        });
    }
}
