using FluentValidation;
using UserService.Application.Dtos;

namespace UserService.Presentation.Validators;

public class LoginDtoValidator:AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(l => l.Email).MaximumLength(320).EmailAddress();
        RuleFor(l => l.Password).MinimumLength(8).MaximumLength(50);
    }
}
