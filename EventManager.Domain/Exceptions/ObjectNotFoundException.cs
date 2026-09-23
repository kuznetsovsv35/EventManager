namespace EventManager.Domain.Exceptions;

/// <summary>
/// Абстрактное исключение - объект по идентификатору не найдено.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public abstract class ObjectNotFoundException<TKey> : ArgumentException where TKey : struct
{
    public TKey ObjectKey { get; }

    protected ObjectNotFoundException(string message, string paramName, TKey key, Exception? innerException)
        : base(message, paramName, innerException) => ObjectKey = key;

    protected ObjectNotFoundException(string message, string paramName, TKey key)
        : this(message, paramName, key, null) { }
}