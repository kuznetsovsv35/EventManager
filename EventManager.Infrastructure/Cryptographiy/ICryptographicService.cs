namespace EventManager.Infrastructure.Cryptography;

/// <summary>
/// Криптографический сервис.
/// </summary>
public interface ICryptographicService
{
    string? EncodeText(string? text);
}