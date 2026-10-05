namespace EventManager.Application.Authorization;

public static class Policies
{
    public static class BookingService
    {
        public static readonly string CancelBooking = $"{nameof(BookingService)}.{nameof(CancelBooking)}";
    }
}