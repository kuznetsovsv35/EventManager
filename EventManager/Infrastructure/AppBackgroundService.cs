using EventManager.Application.Interfaces;

namespace EventManager.Infrastructure;

public class AppBackgroundService(IServiceProvider provider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope = provider.CreateAsyncScope();
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
            while(!stoppingToken.IsCancellationRequested)
            {

                await Task.Delay(2000);
            }
        }
        finally
        {
            await scope.DisposeAsync();
        }
    }
}