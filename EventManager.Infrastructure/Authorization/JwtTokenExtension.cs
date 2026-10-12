using System.Text;
using EventManager.Infrastructure.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EventManager.Infrastructure.Authentication;

public static class JwtTokenExtension
{
    public static TokenValidationParameters Configure(this TokenValidationParameters parameters, JwtSettings settings)
    {
        parameters.ValidateIssuer = true;
        parameters.ValidateAudience = true;
        parameters.ValidateLifetime = true;
        parameters.ValidateIssuerSigningKey = true;

        parameters.ValidIssuer = settings.Issuer;
        parameters.ValidAudience = settings.Audience;
        parameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));
        return parameters;
    }
}