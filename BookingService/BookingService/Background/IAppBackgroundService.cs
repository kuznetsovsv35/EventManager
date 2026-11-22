using BookingService.Domain;

namespace BookingService.Background;

public enum BackgroundServiceStatus
{
    Stopped,
    Starting,
    Running,
    Stopping,
}

public interface IAppBackgroundService : IHostedService
{
    BackgroundServiceStatus Status { get; }
    event EventHandler<Booking> ProcessBooking;
    event EventHandler<BackgroundServiceStatus> StatusChanged;
}