using Microsoft.Extensions.Hosting;
using EventManager.Domain.ValueObjects;

namespace EventManager.Infrastructure.Services;

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