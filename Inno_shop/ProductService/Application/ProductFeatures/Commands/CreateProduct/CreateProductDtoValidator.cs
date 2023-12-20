using FluentValidation;
using ProductService.Application.Dtos;

namespace ProductService.Application.ProductFeatures.Commands.AddProduct;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(p => p.Name).MinimumLength(3).MaximumLength(50);
        RuleFor(p => p.Description).MinimumLength(3).MaximumLength(500);
        RuleFor(p => p.Price).GreaterThan(0);
    }
}
