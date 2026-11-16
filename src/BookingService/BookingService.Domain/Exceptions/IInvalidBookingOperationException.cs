using Authorization;
using Authorization.Exceptions;

namespace BookingService.Domain.Exceptions;

/// <summary>
/// Исключение - неверная операция с бронирования.
/// </summary>
public class InvalidBookingOperation : UserOperationException
{
    public Guid BookingId { get; }

    public string Status { get; }

    public InvalidBookingOperation(string message, Booking booking, ICurrentUser user)
        : this(message, booking, user, null) { }

    public InvalidBookingOperation(string message, Booking booking, ICurrentUser user, Exception? innerException)
        : base(message, user, innerException)
    {
        BookingId = booking.Id;
        Status = booking.Status.ToString();
    }
}