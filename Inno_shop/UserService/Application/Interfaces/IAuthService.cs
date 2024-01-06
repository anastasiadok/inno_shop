using UserService.Application.Dtos;

namespace UserService.Application.Interfaces;

public interface IAuthService
{
    Task<string> Register(UserRegisterDto registerDto);
    Task<LoginResponseDto> Login(LoginDto loginModel);
    Task<string> ForgotPassword(string email);
    Task ResetPassword(ResetPasswordDto model);
}
