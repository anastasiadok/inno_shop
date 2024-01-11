using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UserServiceTests.IntegrationTests.Helpers;

public static class JwtGenerator
{
    private static IConfigurationRoot _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

    public static async Task<string> GenerateJwt(string email, string id)
    {
        var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, id),
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
            expires: now.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
