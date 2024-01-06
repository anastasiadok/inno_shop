using FluentValidation;
using UserService.Application.Dtos;

namespace UserService.Presentation.Validators;

public class RefreshDtoValidator:AbstractValidator<RefreshDto>
{
    public RefreshDtoValidator()
    {
        RuleFor(r => r.AccessToken).MaximumLength(2048);
        RuleFor(r => r.RefreshToken).MaximumLength(36);
    }
}
