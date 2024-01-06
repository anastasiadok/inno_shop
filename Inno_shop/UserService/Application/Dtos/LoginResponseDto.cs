namespace UserService.Application.Dtos;

public record LoginResponseDto(
    string JwtToken,
    DateTime Expiration, 
    string RefreshToken,
    Guid UserId);
