using Microsoft.Extensions.Configuration;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;
using UserService.Application.Services;
using UserService.Domain.Exceptions;
using UserService.Infrastructure.Interfaces;
using UserServiceTests.UnitTests.Mocks;

namespace UserServiceTests.UnitTests.Services;

[Collection("UserUnit")]
public class AuthTests
{
    private readonly IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    private readonly IUserRepository mockUserRepo = MockUserRepository.GetUserRepository().Object;

    private readonly ITokenService mockTokenService;
    private readonly IAuthService mockAuthService;

    public AuthTests()
    {
        mockTokenService = new TokenService(mockUserRepo, configuration);
        mockAuthService = new AuthService(mockUserRepo, mockTokenService);
    }

    [Fact]
    public void RegisterValidTest()
    {
        UserRegisterDto dto = new("name", "string@gmail.com", "password");

        var token = mockAuthService.Register(dto).Result;
        var items = mockUserRepo.GetAllAsync().Result;

        Assert.NotEmpty(token);
        Assert.Equal(4, items.Count());

        MockUserRepository.ResetData();
    }

    [Fact]
    public void RegisterWithUsedEmailTest()
    {
        UserRegisterDto dto = new("name", "john@gmail.com", "password");

        var act = () => mockAuthService.Register(dto);

        var exception = Assert.ThrowsAsync<BadRequestException>(act).Result;
        Assert.Equal("Email is already in use.", exception.Message);
    }

    [Fact]
    public void LoginValidTest()
    {
        LoginDto dto = new("john@gmail.com", "password");

        var result = mockAuthService.Login(dto).Result;
        var item = mockUserRepo.GetByEmailAsync("john@gmail.com").Result;

        Assert.Equal(item.Id, result.UserId);

        MockUserRepository.ResetData();
    }

    [Fact]
    public void LoginNotExistingUserTest()
    {
        LoginDto dto = new("johhhhhn@gmail.com", "password");

        var act = () => mockAuthService.Login(dto);

        var exception = Assert.ThrowsAsync<NotFoundException>(act).Result;
        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public void LoginInvalidPasswordTest()
    {
        LoginDto dto = new("john@gmail.com", "passssssssssword");

        var act = () => mockAuthService.Login(dto);

        var exception = Assert.ThrowsAsync<BadRequestException>(act).Result;
        Assert.Equal("Invalid password.", exception.Message);
    }

    [Fact]
    public void LoginWithNotConfirmedEmailTest()
    {
        LoginDto dto = new("nameee@gmail.com", "password");

        var act = () => mockAuthService.Login(dto);

        var exception = Assert.ThrowsAsync<BadRequestException>(act).Result;
        Assert.Equal("Email is not confirmed.", exception.Message);
    }

    [Fact]
    public void ForgotPasswordValidTest()
    {
        var token = mockAuthService.ForgotPassword("john@gmail.com").Result;

        Assert.NotEmpty(token);

        MockUserRepository.ResetData();
    }

    [Fact]
    public void ForgotPasswordNotExistingUserTest()
    {
        var act = () => mockAuthService.ForgotPassword("johhhhhn@gmail.com");

        var exception = Assert.ThrowsAsync<NotFoundException>(act).Result;
        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public void ResetPasswordValidTest()
    {
        ResetPasswordDto dto = new("nameee@gmail.com", "newpassword", "11111111-1111-1111-1111-111111111111");

        var result = mockAuthService.ResetPassword(dto).IsCompletedSuccessfully;

        Assert.True(result);

        MockUserRepository.ResetData();
    }

    [Fact]
    public void ResetPasswordNotExistingUserTest()
    {
        ResetPasswordDto dto = new("nameeeeeeeeeee@gmail.com", "ppppassword", "11111111-1111-1111-1111-111111111111");

        var act = () => mockAuthService.ResetPassword(dto);

        var exception = Assert.ThrowsAsync<NotFoundException>(act).Result;
        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public void ResetPasswordInvalidResetTokenTest()
    {
        ResetPasswordDto dto = new("nameee@gmail.com", "password", "11111111-0000-1111-1111-111111111111");

        var act = () => mockAuthService.ResetPassword(dto);

        var exception = Assert.ThrowsAsync<BadRequestException>(act).Result;
        Assert.Equal("Invalid token.", exception.Message);
    }

    [Fact]
    public void ResetPasswordExpiredResetTokenTest()
    {
        ResetPasswordDto dto = new("nameee@gmail.com", "password", "11111111-1111-1111-1111-111111111111");

        var act = () => mockAuthService.ResetPassword(dto);
        Thread.Sleep(5000);

        var exception = Assert.ThrowsAsync<BadRequestException>(act).Result;
        Assert.Equal("Reset time is out.", exception.Message);
    }
}
