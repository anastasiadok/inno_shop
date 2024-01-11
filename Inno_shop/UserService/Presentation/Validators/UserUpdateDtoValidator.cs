using FluentValidation;
using UserService.Application.Dtos;

namespace UserService.Presentation.Validators;

public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateDtoValidator()
    {
        RuleFor(u => u.Name).MinimumLength(4).MaximumLength(50);
    }
}
