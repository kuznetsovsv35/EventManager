namespace EventManager.Application.Authorization;

public static class Policies
{
    public static class EventService
    {
        public static readonly string CreateEvent = $"{nameof(EventService)}.{nameof(CreateEvent)}";
        public static readonly string DeleteEvent = $"{nameof(EventService)}.{nameof(DeleteEvent)}";
        public static readonly string GetEvents = $"{nameof(EventService)}.{nameof(GetEvents)}";
        public static readonly string ModifyEvent = $"{nameof(EventService)}.{nameof(ModifyEvent)}";
    }

    public static class BookingService
    {
        public static readonly string CreateBooking = $"{nameof(BookingService)}.{nameof(CreateBooking)}";
        public static readonly string GetBooking = $"{nameof(BookingService)}.{GetBooking}";
        public static readonly string CancelBooking = $"{nameof(BookingService)}.{nameof(CancelBooking)}";
    }
}