using FluentValidation;
using UserService.Application.Dtos;

namespace UserService.Presentation.Validators;

public class UserRegisterDtoValidator : AbstractValidator<UserRegisterDto>
{
    public UserRegisterDtoValidator()
    {
        RuleFor(u => u.Name).MinimumLength(4).MaximumLength(50);
        RuleFor(u => u.Email).MaximumLength(320).EmailAddress();
        RuleFor(u => u.Password).MinimumLength(8).MaximumLength(50);
    }
}
