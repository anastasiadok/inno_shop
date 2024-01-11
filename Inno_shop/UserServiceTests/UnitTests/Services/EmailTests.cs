using Microsoft.Extensions.Configuration;
using UserService.Application.Services;
using UserService.Domain.Exceptions;
using UserServiceTests.UnitTests.Mocks;

namespace UserServiceTests.UnitTests.Services;

public class EmailTests
{
    [Fact]
    public async Task ConfirmEmailValidTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var service = new EmailService(mockUserRepo, configuration);

        var result = service.ConfirmEmailByToken("nameee@gmail.com", "11111111-2222-3333-4444-555555555555").IsCompletedSuccessfully;
        var item = await mockUserRepo.GetByEmailAsync("nameee@gmail.com");

        Assert.True(result);
        Assert.True(item.IsEmailConfirmed);
    }

    [Fact]
    public async Task ConfirmEmailNotExistingUserTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var service = new EmailService(mockUserRepo, configuration);

        var act = () => service.ConfirmEmailByToken("nameeeeeeeee@gmail.com", "11111111-2222-3333-4444-555555555555");

        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal("User not found.", exception.Message);
    }

    [Fact]
    public async Task ConfirmEmailInvalidTokenTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var service = new EmailService(mockUserRepo, configuration);

        var act = () => service.ConfirmEmailByToken("nameee@gmail.com", "00000000-2222-3333-4444-555555555555");

        var exception = await Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal("Invalid token.", exception.Message);
    }

    [Fact]
    public async Task ConfirmEmailAlreadyConfirmedTest()
    {
        var mockUserRepo = MockUserRepository.GetUserRepository().Object;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var service = new EmailService(mockUserRepo, configuration);

        var act = () => service.ConfirmEmailByToken("john@gmail.com", "11111111-2222-3333-4444-555555555555");

        var exception = await Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal("Email is already confirmed.", exception.Message);
    }
}
