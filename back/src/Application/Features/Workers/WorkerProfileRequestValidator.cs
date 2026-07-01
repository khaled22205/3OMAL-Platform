using FluentValidation;

namespace Application.Features.Workers;

public class WorkerProfileRequestValidator : AbstractValidator<WorkerProfileRequest>
{
    public WorkerProfileRequestValidator()
    {
        RuleFor(x => x.Biography).MaximumLength(2000);
        RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 70);
        RuleFor(x => x.HourlyRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StartingPrice).GreaterThanOrEqualTo(0);
    }
}
