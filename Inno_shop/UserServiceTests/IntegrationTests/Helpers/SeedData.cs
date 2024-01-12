using Microsoft.Extensions.DependencyInjection;
using UserService;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Infrastructure.Contexts;

namespace UserServiceTests.IntegrationTests.Helpers;

public static class SeedData
{
    public static void ResetData()
    {
        using var scope = new CustomFactory<Program>().Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        PopulateTestData(appContext);
    }
    
    public static void PopulateTestData(UserDbContext dbContext)
    {
        dbContext.Users.RemoveRange(dbContext.Users.ToList());

        byte[] hash, salt;
        AuthService.CreatePasswordHashAndSalt("password", out hash, out salt);
        
        dbContext.Users.AddRange(new List<User>() {
        new User
        {
            Id = new("136DA01F-9ABD-4d9d-80C7-02AF85C822A2"),
            Name = "John Doe Happy man",
            Email = "john@gmail.com",
            EmailConfirmToken = "11111111-2222-3333-4444-555555555555",
            IsEmailConfirmed = true,
            PasswordHash = hash,
            PasswordSalt = salt,
            RefreshToken = "11111111-1111-1111-4444-555555555555",
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
            RefreshToken = "11111111-1111-1111-4444-555555555555",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
            ResetPasswordToken = "11111111-1111-1111-1111-111111111111",
            ResetPasswordTokenExpiry = DateTime.UtcNow.AddSeconds(5)
        },
        new User
        {
            Id = new("336DA01F-9ABD-4d9d-80C7-02AF85C822A2"),
            Name = "nameee",
            Email = "nameee@gmail.com",
            EmailConfirmToken = "11111111-2222-3333-4444-555555555555",
            IsEmailConfirmed = false,
            PasswordHash = hash,
            PasswordSalt = salt,
            RefreshToken = "11111111-1111-1111-4444-555555555555",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
            ResetPasswordToken = "11111111-1111-1111-1111-111111111111",
            ResetPasswordTokenExpiry = DateTime.UtcNow.AddSeconds(5)
        }});

        dbContext.SaveChanges();
    }
}
