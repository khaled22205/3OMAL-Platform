using FluentValidation;
using src.DTOs.Bookings;

namespace src.Validators.Bookings;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.WorkerProfileId).GreaterThan(0);
        RuleFor(x => x.ScheduledAt).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}