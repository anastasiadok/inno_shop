using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Infrastructure.Interfaces;

namespace UserService.Application.Services;

public class TokenService : ITokenService
{
    private readonly IUserRepository _repository;

    private readonly IConfiguration _configuration;

    public TokenService(IUserRepository repository, IConfiguration configuration)
    {
        _configuration = configuration;
        _repository = repository;
    }

    public async Task<LoginResponseDto> Refresh(RefreshDto refreshModel)
    {
        var principal = GetPrincipalFromExpiredToken(refreshModel.AccessToken);
        if (principal?.Identity?.Name is null)
            throw new BadRequestException("Invalid jwt.");

        var user = await _repository.GetByEmailAsync(principal.Identity.Name) 
            ?? throw new NotFoundException(nameof(User));
        
        if (user.RefreshToken != refreshModel.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new BadRequestException("Invalid refresh token.");

        var token =await GenerateJwt(principal.Identity.Name);

        return new(
            JwtToken: new JwtSecurityTokenHandler().WriteToken(token),
            Expiration: token.ValidTo,
            RefreshToken: refreshModel.RefreshToken,
            UserId: user.Id
        );
    }

    public async Task RevokeRefreshTokenByEmail(string userEmail)
    {
        if (userEmail.IsNullOrEmpty())
            throw new UnauthorizedException("Invalid email.");

        var user = await _repository.GetByEmailAsync(userEmail) 
            ?? throw new NotFoundException(nameof(User));

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        await _repository.UpdateAsync(user);
    }

    public string GenerateToken() => Guid.NewGuid().ToString();

    public async Task<JwtSecurityToken> GenerateJwt(string email)
    {
        var user = await _repository.GetByEmailAsync(email) ?? throw new NotFoundException(nameof(User));

        var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString() ),
                new(ClaimTypes.Name, email)
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"] 
            ?? throw new InvalidOperationException("Key not configured")));

        var now = DateTime.UtcNow;

        var jwt = new JwtSecurityToken(
            issuer: _configuration["Jwt:ValidIssuer"],
            audience: _configuration["Jwt:ValidAudience"],
            notBefore: now,
            claims: claims,
            expires: now.AddHours(Convert.ToDouble(_configuration["Jwt:DurationInHours"])),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return jwt;
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var key = _configuration["JWT:Key"] ?? throw new InvalidOperationException("Key not configured");

        var validation = new TokenValidationParameters
        {
            ValidIssuer = _configuration["JWT:ValidIssuer"],
            ValidAudience = _configuration["JWT:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        };

        return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
    }
}
