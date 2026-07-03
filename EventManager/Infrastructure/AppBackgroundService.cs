using EventManager.Application.Interfaces;

namespace EventManager.Infrastructure;

public class AppBackgroundService(IServiceProvider provider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(2000);
        }
    }
}