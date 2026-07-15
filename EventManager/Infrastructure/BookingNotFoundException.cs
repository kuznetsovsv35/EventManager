namespace EventManager.Infrastructure;

public class BookingNotFoundException : ObjectNotFoundException<Guid>
{
    internal BookingNotFoundException(string paramName, Guid bookingId)
        : this(paramName, bookingId, null) {}
        
    internal BookingNotFoundException(string paramName, Guid bookingId, Exception? innerException)
        : base("Бронирование не найдено", paramName, bookingId) {}
}