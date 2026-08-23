namespace EventManager.Infrastructure;

public class BookingNotFoundException : ObjectNotFoundException<Guid>
{
    internal BookingNotFoundException(Guid bookingId, string paramName)
        : this(bookingId, paramName, null) { }

    internal BookingNotFoundException(Guid bookingId, string paramName, Exception? innerException)
        : base("Бронирование не найдено", paramName, bookingId) { }
}