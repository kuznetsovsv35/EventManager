namespace BookingService.Application.Security;

public static class Policy
{
    public const string CreateBooking = $"{nameof(BookingService)}.{nameof(CreateBooking)}";
    public const string GetBooking = $"{nameof(BookingService)}.{nameof(GetBooking)}";
    public const string CancelBooking = $"{nameof(BookingService)}.{nameof(CancelBooking)}";
}
