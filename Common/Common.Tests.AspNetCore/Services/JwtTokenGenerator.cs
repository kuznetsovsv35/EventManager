using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Authorization.AspNetCore.Extensions;
using Authorization.Configuration;
using Authorization.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Common.Tests.AspNetCore.Services;

public class JwtTokenGenerator(IOptions<JwtSettings> options) : ITokenGenerator
{
    public JwtSettings Settings => options.Value;

    readonly JwtSecurityTokenHandler _jwtHandler = new();

    readonly TokenValidationParameters _tokenValidationParams = new TokenValidationParameters().Configure(options.Value);

    public string GenerateTokens(Guid id, string login, UserRole role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, id.ToString()),
            new(ClaimTypes.Name, login),
            new(ClaimTypes.Role, role.ToString()),
        };

        var token = new JwtSecurityToken
        (
            issuer: Settings.Issuer,
            audience: Settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Settings.AccessTokenExpiredAfter),
            signingCredentials: new SigningCredentials(_tokenValidationParams.IssuerSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return _jwtHandler.WriteToken(token);
    }
}
