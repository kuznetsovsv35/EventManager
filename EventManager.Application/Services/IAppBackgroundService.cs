using EventManager.Domain.ValueObjects;

namespace EventManager.Application.Services;

public enum BackgroundServiceStatus
{
    Stopped,
    Starting,
    Running,
    Stopping,
}

public interface IAppBackgroundService
{
    BackgroundServiceStatus Status { get; }

    event EventHandler<Booking> ProcessBooking;
}