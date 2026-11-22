using BookingService.Domain;

namespace BookingService.Application.DTO;

public static class Extensions
{
    public static BookingInfo ToInfo(this Booking booking)
        => new()
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt,
        };
}
