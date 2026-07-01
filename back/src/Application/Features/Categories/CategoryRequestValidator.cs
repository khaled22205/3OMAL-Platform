using FluentValidation;

namespace Application.Features.Categories;

public class CategoryRequestValidator : AbstractValidator<CategoryRequest>
{
    public CategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SeoUrl).MaximumLength(200);
    }
}
