using Microsoft.Extensions.Configuration;
using UserService.Application.Interfaces;
using UserService.Application.Services;
using UserService.Domain.Exceptions;
using UserService.Infrastructure.Interfaces;
using UserServiceTests.UnitTests.Mocks;

namespace UserServiceTests.UnitTests.Services;

[Collection("UserUnit")]
public class EmailTests
{
    private readonly IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    private readonly IUserRepository mockUserRepo = MockUserRepository.GetUserRepository().Object;

    private readonly IEmailService mockEmailService;

    public EmailTests()
    {
        mockEmailService = new EmailService(mockUserRepo, configuration);
    }

    [Fact]
    public void ConfirmEmailValidTest()
    {
        var result = mockEmailService.ConfirmEmailByToken("nameee@gmail.com", "11111111-2222-3333-4444-555555555555").IsCompletedSuccessfully;
        var item = mockUserRepo.GetByEmailAsync("nameee@gmail.com").Result;

        Assert.True(result);
        Assert.True(item.IsEmailConfirmed);

        MockUserRepository.ResetData();
    }

    [Fact]
    public void ConfirmEmailNotExistingUserTest()
    {
        var act = () => mockEmailService.ConfirmEmailByToken("nameeeeeeeee@gmail.com", "11111111-2222-3333-4444-555555555555");

        var exception = Assert.ThrowsAsync<NotFoundException>(act).Result;
        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public void ConfirmEmailInvalidTokenTest()
    {
        var act = () => mockEmailService.ConfirmEmailByToken("nameee@gmail.com", "00000000-2222-3333-4444-555555555555");

        var exception = Assert.ThrowsAsync<BadRequestException>(act).Result;
        Assert.Equal("Invalid token.", exception.Message);
    }

    [Fact]
    public void ConfirmEmailAlreadyConfirmedTest()
    {
        var act = () => mockEmailService.ConfirmEmailByToken("john@gmail.com", "11111111-2222-3333-4444-555555555555");

        var exception = Assert.ThrowsAsync<BadRequestException>(act).Result;
        Assert.Equal("Email is already confirmed.", exception.Message);
    }
}
