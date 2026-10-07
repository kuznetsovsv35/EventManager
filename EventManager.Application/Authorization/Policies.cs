namespace EventManager.Application.Authorization;

public static class Policies
{
    public static class BookingService
    {
        public static readonly string CreateBooking = $"{nameof(BookingService)}.{nameof(CreateBooking)}";
        public static readonly string GetBooking = $"{nameof(BookingService)}.{GetBooking}";
        public static readonly string CancelBooking = $"{nameof(BookingService)}.{nameof(CancelBooking)}";
    }
}