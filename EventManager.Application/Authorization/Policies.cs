namespace EventManager.Application.Authorization;

public static class Policies
{
    public static class EventService
    {
        public const string CreateEvent = $"{nameof(EventService)}.{nameof(CreateEvent)}";
        public const string DeleteEvent = $"{nameof(EventService)}.{nameof(DeleteEvent)}";
        public const string GetEvents = $"{nameof(EventService)}.{nameof(GetEvents)}";
        public const string ModifyEvent = $"{nameof(EventService)}.{nameof(ModifyEvent)}";
    }

    public static class BookingService
    {
        public const string CreateBooking = $"{nameof(BookingService)}.{nameof(CreateBooking)}";
        public const string GetBooking = $"{nameof(BookingService)}.{nameof(GetBooking)}";
        public const string CancelBooking = $"{nameof(BookingService)}.{nameof(CancelBooking)}";
    }

    public static class UserService
    {
        public const string ChangePassword = $"{nameof(UserService)}.{nameof(ChangePassword)}";
        public const string ModifyUser = $"{nameof(UserService)}.{nameof(ModifyUser)}";
        public const string DeleteUser = $"{nameof(UserService)}.{nameof(DeleteUser)}";
        public const string RegisterUser = $"{nameof(UserService)}.{nameof(RegisterUser)}";
    }
}