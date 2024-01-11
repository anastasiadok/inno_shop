using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using UserService.Application.Dtos;
using UserService.Domain.Exceptions;
using UserServiceTests.UnitTests.Mocks;

namespace UserServiceTests.UnitTests.Services;

public class TokenTests
{
    [Fact]
    public async Task RefreshValidTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        UServices.TokenService tokenService = new(mockUserRepo, configuration);

        var token = await tokenService.GenerateJwt("john@gmail.com");
        RefreshDto dto = new(new JwtSecurityTokenHandler().WriteToken(token),"11111111-1111-1111-4444-555555555555");

        var response = await tokenService.Refresh(dto);
        var item = await mockUserRepo.GetByEmailAsync("john@gmail.com");

        Assert.Equal(item.RefreshToken, response.RefreshToken);
    }

    [Fact]
    public async Task RefreshInvalidRefreshTokenTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        UServices.TokenService tokenService = new(mockUserRepo, configuration);

        var token = await tokenService.GenerateJwt("john@gmail.com");
        RefreshDto dto = new(new JwtSecurityTokenHandler().WriteToken(token), "11112221-1111-1111-4444-555555555555");

        var act = () => tokenService.Refresh(dto);

        var exception = await Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal("Invalid refresh token.", exception.Message);
    }

    [Fact]
    public async Task RevokeValidTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        UServices.TokenService tokenService = new(mockUserRepo, configuration);

        var response = tokenService.RevokeRefreshTokenByEmail("john@gmail.com").IsCompletedSuccessfully;
        var item = await mockUserRepo.GetByEmailAsync("john@gmail.com");

        Assert.True(response);
        Assert.Null(item.RefreshToken);
        Assert.Null(item.RefreshTokenExpiry);
    }

    [Fact]
    public async Task RevokeWithEmptyEmailTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        UServices.TokenService tokenService = new(mockUserRepo, configuration);

        var act = () => tokenService.RevokeRefreshTokenByEmail("");

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(act);
        Assert.Equal("Invalid email.", exception.Message);
    }

    [Fact]
    public async Task RevokeNotExistingUserTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        UServices.TokenService tokenService = new(mockUserRepo, configuration);

        var act = () => tokenService.RevokeRefreshTokenByEmail("aaaaa@gmail.com");

        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public async Task GenerateJwtValidTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        UServices.TokenService tokenService = new(mockUserRepo, configuration);

        var token = new JwtSecurityTokenHandler().WriteToken(await tokenService.GenerateJwt("john@gmail.com"));

        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task GenerateJwtNotExistingUserTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        UServices.TokenService tokenService = new(mockUserRepo, configuration);

        var act = () => tokenService.GenerateJwt("johhhhhhhn@gmail.com");

        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal("User not found.", exception.Message);
    }

}
