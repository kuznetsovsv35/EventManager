using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventManager.Application.DataTransferObjects;
using EventManager.Infrastructure.Authorization;
using EventManager.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventManager.Infrastructure.Services;

public class JwtTokenService(IOptions<JwtSettings> options) : ITokenService
{
    public JwtSettings Settings => options.Value;
   
    readonly JwtSecurityTokenHandler _jwtHandler = new();

    readonly TokenValidationParameters _tokenValidationParams = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = options.Value.Issuer,
        ValidAudience = options.Value.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SigningKey))
    };

    public string GenerateAccessToken(UserInfo userInfo, IEnumerable<Claim>? extraClaims)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
            new(ClaimTypes.Name, userInfo.Login),
            new(ClaimTypes.Role, userInfo.Role.ToString()),
        };

        if (extraClaims is not null)
            claims.AddRange(extraClaims);

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

    public string GenerateRefreshToken() => Guid.NewGuid().ToString("N");

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        try
        {
            var principal = _jwtHandler.ValidateToken(token, _tokenValidationParams, out var _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public bool ValidateRefreshToken(string token, string storedToken) => token == storedToken;
}
