using Mapster;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;
using UserService.Domain.Exceptions;
using UserService.Domain.Entities;
using UserService.Infrastructure.Interfaces;

namespace UserService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repository;

    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository repository, ITokenService tokenService)
    {
        _repository = repository;
        _tokenService = tokenService;
    }

    public async Task<string> Register(UserRegisterDto registerDto)
    {
        bool isFree = await _repository.CheckEmailIsFree(registerDto.Email);
        if (!isFree)
            throw new BadRequestException("Email is already in use.");

        var user = registerDto.Adapt<User>();
        CreatePasswordHashAndSalt(registerDto.Password, out byte[] hash, out byte[] salt);
        var token = _tokenService.GenerateToken();

        user.Id = Guid.NewGuid();
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.EmailConfirmToken = token;

        await _repository.AddAsync(user);

        return token;
    }

    public async Task<LoginResponseDto> Login(LoginDto loginModel)
    {
        User user = await _repository.GetByEmailAsync(loginModel.Email) ?? throw new NotFoundException(nameof(User));

        if (!CheckPassword(loginModel.Password, user.PasswordHash, user.PasswordSalt))
            throw new BadRequestException("Invalid password.");

        if (!user.IsEmailConfirmed)
            throw new BadRequestException("Email is not confirmed.");

        JwtSecurityToken jwt = await _tokenService.GenerateJwt(loginModel.Email);
        var refreshToken = _tokenService.GenerateToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _repository.UpdateAsync(user);

        return new(
            JwtToken: new JwtSecurityTokenHandler().WriteToken(jwt),
            Expiration: jwt.ValidTo,
            RefreshToken: refreshToken,
            UserId: user.Id
        );
    }

    public async Task<string> ForgotPassword(string email)
    {
        var user = await _repository.GetByEmailAsync(email)
            ?? throw new NotFoundException(nameof(User));

        var token = _tokenService.GenerateToken();

        user.ResetPasswordToken = token;
        user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _repository.UpdateAsync(user);

        return token;
    }

    public async Task ResetPassword(ResetPasswordDto model)
    {
        var user = await _repository.GetByEmailAsync(model.Email)
            ?? throw new NotFoundException(nameof(User));

        if (user.ResetPasswordToken != model.ResetToken)
            throw new BadRequestException("Invalid token.");

        if (user.ResetPasswordTokenExpiry < DateTime.UtcNow)
            throw new BadRequestException("Reset time is out.");

        CreatePasswordHashAndSalt(model.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;
        user.ResetPasswordToken = null;
        user.ResetPasswordTokenExpiry = null;

        await _repository.UpdateAsync(user);
    }

    public static void CreatePasswordHashAndSalt(string password, out byte[] userPasswordHash, out byte[] userPasswordSalt)
    {
        using var hmac = new HMACSHA512();
        userPasswordSalt = hmac.Key;
        userPasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    public static bool CheckPassword(string password, byte[] userPasswordHash, byte[] userPasswordSalt)
    {
        using var hmac = new HMACSHA512(userPasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(userPasswordHash);
    }
}
