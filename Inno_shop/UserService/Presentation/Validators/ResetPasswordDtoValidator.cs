using FluentValidation;
using UserService.Application.Dtos;

namespace UserService.Presentation.Validators;

public class ResetPasswordDtoValidator:AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(r => r.Email).MaximumLength(320).EmailAddress();
        RuleFor(r => r.NewPassword).MinimumLength(8).MaximumLength(50);
        RuleFor(r => r.ResetToken).MaximumLength(36);
    }
}
