namespace EventManager.Application.Interfaces;

public enum BackgroundServiceStatus
{
    Stopped,
    Starting,
    Runing,
    Stopping,
}

public interface IAppBackgroundService : IHostedService
{
    BackgroundServiceStatus Status { get; }
}