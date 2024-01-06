using FluentValidation;
using ProductService.Application.Dtos;

namespace ProductService.Presentation.Validators;

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(p => p.Name).MinimumLength(3).MaximumLength(50);
        RuleFor(p => p.Description).MinimumLength(3).MaximumLength(500);
        RuleFor(p => p.Price).GreaterThan(0);
    }
}
