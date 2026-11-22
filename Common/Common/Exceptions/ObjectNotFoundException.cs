namespace Common.Exceptions;

/// <summary>
/// Абстрактное исключение - объект по идентификатору не найдено.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public abstract class ObjectNotFoundException<TKey>(string message, string paramName, TKey key, Exception? innerException) : ArgumentException(message, paramName, innerException) where TKey : struct
{
    public TKey ObjectKey { get; } = key;

    protected ObjectNotFoundException(string message, string paramName, TKey key)
        : this(message, paramName, key, null) { }
}