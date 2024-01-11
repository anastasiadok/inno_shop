namespace UserService.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;
    public string? EmailConfirmToken { get; set; }
    public bool IsEmailConfirmed { get; set; } = false;

    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    public string? ResetPasswordToken { get; set; }
    public DateTime? ResetPasswordTokenExpiry { get; set; }
}
