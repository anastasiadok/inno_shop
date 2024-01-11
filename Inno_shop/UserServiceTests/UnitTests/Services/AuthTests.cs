using Microsoft.Extensions.Configuration;
using UserService.Application.Dtos;
using UserService.Application.Services;
using UserService.Domain.Exceptions;
using UserServiceTests.UnitTests.Mocks;

namespace UserServiceTests.UnitTests.Services;

public class AuthTests
{

    [Fact]
    public async Task RegisterValidTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var mockTokenService = new TokenService(mockUserRepo, configuration);
        var service = new AuthService(mockUserRepo, mockTokenService);

        UserRegisterDto dto = new("name", "string@gmail.com", "password");

        var token = await service.Register(dto);
        var items = await mockUserRepo.GetAllAsync();

        Assert.NotEmpty(token);
        Assert.Equal(4, items.Count());
    }

    [Fact]
    public async Task RegisterWithUsedEmailTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var mockTokenService = new TokenService(mockUserRepo, configuration);
        var service = new AuthService(mockUserRepo, mockTokenService);

        UserRegisterDto dto = new("name", "john@gmail.com", "password");

        var act = () => service.Register(dto);

        var exception = await Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal("Email is already in use.", exception.Message);
    }

    [Fact]
    public async Task LoginValidTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var mockTokenService = new TokenService(mockUserRepo, configuration);
        var service = new AuthService(mockUserRepo, mockTokenService);

        LoginDto dto = new("john@gmail.com", "password");

        var result = await service.Login(dto);
        var item = await mockUserRepo.GetByEmailAsync("john@gmail.com");

        Assert.Equal(item.Id, result.UserId);
    }

    [Fact]
    public async Task LoginNotExistingUserTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var mockTokenService = new TokenService(mockUserRepo, configuration);
        var service = new AuthService(mockUserRepo, mockTokenService);

        LoginDto dto = new("johhhhhn@gmail.com", "password");

        var act = () => service.Login(dto);

        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public async Task LoginInvalidPasswordTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var mockTokenService = new TokenService(mockUserRepo, configuration);
        var service = new AuthService(mockUserRepo, mockTokenService);

        LoginDto dto = new("john@gmail.com", "passssssssssword");

        var act = () => service.Login(dto);

        var exception = await Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal("Invalid password.", exception.Message);
    }

    [Fact]
    public async Task LoginWithNotConfirmedEmailTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var mockTokenService = new TokenService(mockUserRepo, configuration);
        var service = new AuthService(mockUserRepo, mockTokenService);

        LoginDto dto = new("nameee@gmail.com", "password");

        var act = () => service.Login(dto);

        var exception = await Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal("Email is not confirmed.", exception.Message);
    }

    [Fact]
    public async Task ForgotPasswordValidTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var mockTokenService = new TokenService(mockUserRepo, configuration);
        var service = new AuthService(mockUserRepo, mockTokenService);

        var token = await service.ForgotPassword("john@gmail.com");

        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task ForgotPasswordNotExistingUserTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var mockTokenService = new TokenService(mockUserRepo, configuration);
        var service = new AuthService(mockUserRepo, mockTokenService);

        var act = () => service.ForgotPassword("johhhhhn@gmail.com");

        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public async Task ResetPasswordValidTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var mockTokenService = new TokenService(mockUserRepo, configuration);
        var service = new AuthService(mockUserRepo, mockTokenService);

        ResetPasswordDto dto = new("nameee@gmail.com", "newpassword", "11111111-1111-1111-1111-111111111111");

        var result = service.ResetPassword(dto).IsCompletedSuccessfully;

        Assert.True(result);
    }

    [Fact]
    public async Task ResetPasswordNotExistingUserTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var mockTokenService = new TokenService(mockUserRepo, configuration);
        var service = new AuthService(mockUserRepo, mockTokenService);

        ResetPasswordDto dto = new("nameeeeeeeeeee@gmail.com", "ppppassword", "11111111-1111-1111-1111-111111111111");

        var act = () => service.ResetPassword(dto);

        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public async Task ResetPasswordInvalidResetTokenTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var mockTokenService = new TokenService(mockUserRepo, configuration);
        var service = new AuthService(mockUserRepo, mockTokenService);

        ResetPasswordDto dto = new("nameee@gmail.com", "password", "11111111-0000-1111-1111-111111111111");

        var act = () => service.ResetPassword(dto);

        var exception = await Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal("Invalid token.", exception.Message);
    }

    [Fact]
    public async Task ResetPasswordExpiredResetTokenTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var mockTokenService = new TokenService(mockUserRepo, configuration);
        var service = new AuthService(mockUserRepo, mockTokenService);

        ResetPasswordDto dto = new("nameee@gmail.com", "password", "11111111-1111-1111-1111-111111111111");

        var act = () => service.ResetPassword(dto);
        await Task.Delay(5000);

        var exception = await Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal("Reset time is out.", exception.Message);
    }
}
