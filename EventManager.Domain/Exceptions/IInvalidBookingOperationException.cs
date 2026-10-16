using EventManager.Domain.ValueObjects;

namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение - неверная операция с бронирования.
/// </summary>
public class InvalidBookingOperation : UserOperationException
{
    public Guid BookingId { get; }

    public string Status { get; }

    public InvalidBookingOperation(string message, Booking booking, string login, UserRole role)
        : this(message, booking, login, role, null) { }

    public InvalidBookingOperation(string message, Booking booking, string login, UserRole role, Exception? innerException)
        : base(message, login, role, innerException)
    {
        BookingId = booking.Id;
        Status = booking.Status.ToString();
    }
}