namespace EventManager.Infrastructure.Configuration;

public class JwtSettings
{
    public string Issuer { get; set; } = "Events-API";

    public string Audience { get; set; } = "Events-API-Clients";

    public string SigningKey {get; set; } = string.Empty;

    public int AccessTokenExpiredAfter { get; set; } = 15;

    public int RefreshTokenExpiredAfter { get; set; } = 7;    
}