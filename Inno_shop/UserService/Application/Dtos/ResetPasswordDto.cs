namespace UserService.Application.Dtos;

public record ResetPasswordDto(string Email, string NewPassword, string ResetToken);
