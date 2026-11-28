using Common.Exceptions;

namespace BookingService.Domain.Exceptions;

/// <summary>
/// Исключение - бронь по идентификатору не найдена.
/// </summary>
public class BookingNotFoundException(Guid bookingId, string paramName, Exception? innerException) : ObjectNotFoundException<Guid>("Бронирование не найдено", paramName, bookingId, innerException)
{
    public BookingNotFoundException(Guid bookingId, string paramName)
        : this(bookingId, paramName, null) { }
}