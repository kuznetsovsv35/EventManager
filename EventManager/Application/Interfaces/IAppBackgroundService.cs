namespace EventManager.Application.Interfaces;

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
}