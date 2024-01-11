using Moq;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Infrastructure.Interfaces;

namespace UserServiceTests.UnitTests.Mocks;

public static class MockUserRepository
{
    public static Mock<IUserRepository> GetUserRepository()
    {
        byte[] hash, salt;
        AuthService.CreatePasswordHashAndSalt("password", out hash, out salt);
        List<User> list = new()
        {
            new User
            {
                Id = new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"),
                Name = "John Doe Happy man",
                Email = "john@gmail.com",
                EmailConfirmToken = "11111111-2222-3333-4444-555555555555",
                IsEmailConfirmed = true,
                PasswordHash = hash,
                PasswordSalt = salt,
                RefreshToken ="11111111-1111-1111-4444-555555555555",
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
                ResetPasswordToken = "11111111-1111-1111-1111-111111111111",
                ResetPasswordTokenExpiry = DateTime.UtcNow.AddSeconds(5)
            },
            new User
            {
                Id = new("236DA01F-9ABD-4d9d-80C7-02AF85C822A2"),
                Name = "Adolf",
                Email = "Adolf@gmail.com",
                EmailConfirmToken = "11111111-2222-3333-4444-555555555555",
                IsEmailConfirmed = true,
                PasswordHash = hash,
                PasswordSalt = salt,
                RefreshToken ="11111111-1111-1111-4444-555555555555",
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
                ResetPasswordToken = "11111111-1111-1111-1111-111111111111",
                ResetPasswordTokenExpiry = DateTime.UtcNow.AddSeconds(5)
            },
            new User
            {
                Id =new("336DA01F-9ABD-4d9d-80C7-02AF85C822A2"),
                Name = "nameee",
                Email = "nameee@gmail.com",
                EmailConfirmToken = "11111111-2222-3333-4444-555555555555",
                IsEmailConfirmed = false,
                PasswordHash = hash,
                PasswordSalt = salt,
                RefreshToken ="11111111-1111-1111-4444-555555555555",
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
                ResetPasswordToken = "11111111-1111-1111-1111-111111111111",
                ResetPasswordTokenExpiry = DateTime.UtcNow.AddSeconds(5)
            }
        };

        var mockRepo = new Mock<IUserRepository>();

        mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(list);

        mockRepo.Setup(r => r.AddAsync(It.IsAny<User>())).Returns((User user) =>
        {
            list.Add(user);
            return Task.CompletedTask;
        });

        mockRepo.Setup(r => r.DeleteByIdAsync(It.IsAny<Guid>())).Returns((Guid id) =>
        {
            list.Remove(list.Find(u => u.Id == id));
            return Task.CompletedTask;
        });

        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns((User user) =>
        {
            list.Remove(list.Find(u => u.Id == user.Id));
            list.Add(user);

            return Task.CompletedTask;
        });

        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Guid id) => list.Find(u => u.Id == id));

        mockRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((string email) => list.Find(u => u.Email == email));

        mockRepo.Setup(r => r.CheckEmailIsFree(It.IsAny<string>())).ReturnsAsync((string email) => list.Find(u => u.Email == email) is null);

        return mockRepo;
    }
}
