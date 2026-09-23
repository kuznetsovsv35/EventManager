namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение - бронь по идентификатору не найдена.
/// </summary>
public class BookingNotFoundException : ObjectNotFoundException<Guid>
{
    public BookingNotFoundException(Guid bookingId, string paramName)
        : this(bookingId, paramName, null) { }

    public BookingNotFoundException(Guid bookingId, string paramName, Exception? innerException)
        : base("Бронирование не найдено", paramName, bookingId) { }
}