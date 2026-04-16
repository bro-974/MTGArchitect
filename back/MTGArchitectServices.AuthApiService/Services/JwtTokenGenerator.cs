using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MTGArchitect.Data.Models;
using MTGArchitectServices.AuthApiService.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MTGArchitectServices.AuthApiService.Services;

public interface IJwtTokenGenerator
{
    AuthToken GenerateToken(ApplicationUser user);
}

public sealed record AuthToken(string AccessToken, DateTime ExpiresAtUtc);

public sealed class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
    private readonly JwtOptions jwtOptions = options.Value;

    public AuthToken GenerateToken(ApplicationUser user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationMinutes);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        return new(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
