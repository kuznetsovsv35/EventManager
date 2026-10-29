namespace EventManager.Application.Cryptography;

/// <summary>
/// Компонент проверки пароля.
/// </summary>
public interface IPasswordHasher
{
    string? Hash(string? password);
    bool Verify(string? hash, string? password);
}