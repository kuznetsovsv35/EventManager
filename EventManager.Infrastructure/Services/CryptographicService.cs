using EventManager.Infrastructure.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace EventManager.Infrastructure.Services;

public class CryptographicService : ICryptographicService
{
    public string? EncodeText(string? text)
    {
        if (text is null)
            return null;

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes);
    }
}