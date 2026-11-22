using Authorization;
using Authorization.Exceptions;

namespace BookingService.Domain.Exceptions;

/// <summary>
/// Исключение - неверная операция с бронирования.
/// </summary>
public class InvalidBookingOperationException(string message, Booking booking, ICurrentUser user, Exception? innerException) 
    : UserOperationException(message, user, innerException)
{
    public Guid BookingId { get; } = booking.Id;

    public string Status { get; } = booking.Status.ToString();

    public InvalidBookingOperationException(string message, Booking booking, ICurrentUser user)
        : this(message, booking, user, null) { }
}