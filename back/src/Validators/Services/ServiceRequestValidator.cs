using FluentValidation;
using src.DTOs.Services;

namespace src.Validators.Services;

public class ServiceRequestValidator : AbstractValidator<ServiceRequest>
{
    public ServiceRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.PriceType).Must(t => t is "Fixed" or "Hourly")
            .WithMessage("PriceType must be 'Fixed' or 'Hourly'");
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.EstimatedDurationMinutes).GreaterThan(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
    }
}