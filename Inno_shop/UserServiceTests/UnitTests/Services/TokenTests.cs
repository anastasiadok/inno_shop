using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;
using UserService.Application.Services;
using UserService.Domain.Exceptions;
using UserService.Infrastructure.Interfaces;
using UserServiceTests.UnitTests.Mocks;

namespace UserServiceTests.UnitTests.Services;

[Collection("UserUnit")]
public class TokenTests
{
    private readonly IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    private readonly IUserRepository mockUserRepo = MockUserRepository.GetUserRepository().Object;

    private readonly ITokenService mockTokenService;

    public TokenTests()
    {
        mockTokenService = new TokenService(mockUserRepo, configuration);
    }

    [Fact]
    public void RefreshValidTest()
    {
        var token = mockTokenService.GenerateJwt("john@gmail.com").Result;
        RefreshDto dto = new(new JwtSecurityTokenHandler().WriteToken(token),"11111111-1111-1111-4444-555555555555");

        var response = mockTokenService.Refresh(dto).Result;
        var item = mockUserRepo.GetByEmailAsync("john@gmail.com").Result;

        Assert.Equal(item.RefreshToken, response.RefreshToken);
    }

    [Fact]
    public void RefreshInvalidRefreshTokenTest()
    {
        var token = mockTokenService.GenerateJwt("john@gmail.com").Result;
        RefreshDto dto = new(new JwtSecurityTokenHandler().WriteToken(token), "11112221-1111-1111-4444-555555555555");

        var act = () => mockTokenService.Refresh(dto);

        var exception = Assert.ThrowsAsync<BadRequestException>(act).Result;
        Assert.Equal("Invalid refresh token.", exception.Message);
    }

    [Fact]
    public void RevokeValidTest()
    {
        var response = mockTokenService.RevokeRefreshTokenByEmail("john@gmail.com").IsCompletedSuccessfully;
        var item = mockUserRepo.GetByEmailAsync("john@gmail.com").Result;

        Assert.True(response);
        Assert.Null(item.RefreshToken);
        Assert.Null(item.RefreshTokenExpiry);

        MockUserRepository.ResetData();
    }

    [Fact]
    public void RevokeWithEmptyEmailTest()
    {
        var act = () => mockTokenService.RevokeRefreshTokenByEmail("");

        var exception = Assert.ThrowsAsync<UnauthorizedException>(act).Result;
        Assert.Equal("Invalid email.", exception.Message);
    }

    [Fact]
    public void RevokeNotExistingUserTest()
    {
        var act = () => mockTokenService.RevokeRefreshTokenByEmail("aaaaa@gmail.com");

        var exception = Assert.ThrowsAsync<NotFoundException>(act).Result;
        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public void GenerateJwtValidTest()
    {
        var token = new JwtSecurityTokenHandler().WriteToken(mockTokenService.GenerateJwt("john@gmail.com").Result);

        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateJwtNotExistingUserTest()
    {
        var act = () => mockTokenService.GenerateJwt("johhhhhhhn@gmail.com");

        var exception = Assert.ThrowsAsync<NotFoundException>(act).Result;
        Assert.Equal("User not found.", exception.Message);
    }

}
