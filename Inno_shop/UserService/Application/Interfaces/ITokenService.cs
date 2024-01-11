using System.IdentityModel.Tokens.Jwt;
using UserService.Application.Dtos;

namespace UserService.Application.Interfaces;

public interface ITokenService
{
    Task<LoginResponseDto> Refresh(RefreshDto refreshModel);
    Task RevokeRefreshTokenByEmail(string userEmail);
    string GenerateToken();
    Task<JwtSecurityToken> GenerateJwt(string email);
}
