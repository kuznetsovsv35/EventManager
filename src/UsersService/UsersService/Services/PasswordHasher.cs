using System.Security.Cryptography;
using System.Text;
using UsersService.Application.Cryptography;

namespace UsersService.Services;

public class PasswordHasher : IPasswordHasher
{
    public string? Hash(string? password)
    {
        if (password is null || password.Length == 0)
            return null;

        return EncodeText(password);
    }

    public bool Verify(string? hash, string? password)
    {
        if (hash is null || hash.Length == 0 || password is null || password.Length == 0)
            return false;

        return EncodeText(password) == hash;
    }

    string EncodeText(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes);
    }
}
