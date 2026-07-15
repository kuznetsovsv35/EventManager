using EventManager.Application.Interfaces;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure;

public class AppBackgroundService(
    IServiceScopeFactory scopeFactory,
    IAsyncQueue<Booking> bookingQueue,
    ILogger<AppBackgroundService> logger) : BackgroundService, IAppBackgroundService
{
    readonly IServiceScope _serviceScope = scopeFactory.CreateScope();
    BackgroundServiceStatus _status = BackgroundServiceStatus.Stopped;

    public BackgroundServiceStatus Status => _status;

    public override Task StartAsync(CancellationToken cancellation)
    {
        Interlocked.CompareExchange(ref _status, BackgroundServiceStatus.Starting, BackgroundServiceStatus.Stopped);
        return base.StartAsync(cancellation);
    }

    public override Task StopAsync(CancellationToken cancellation)
    {
        Interlocked.CompareExchange(ref _status, BackgroundServiceStatus.Stopping, BackgroundServiceStatus.Running);
        return base.StopAsync(cancellation);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Interlocked.Exchange(ref _status, BackgroundServiceStatus.Runing);
        logger.LogInformation("Старт фонового процесса обработки ...");
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessBooking(await bookingQueue.Dequeue(stoppingToken), stoppingToken);
                }
                catch (OperationCanceledException canceled) when (canceled.CancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка обработки.");
                    if (!stoppingToken.IsCancellationRequested)
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }
        }
        finally
        {
            Interlocked.Exchange(ref _status, BackgroundServiceStatus.Stopped);
            logger.LogInformation("Завершение фонового процесса обработки...");
        }
    }

    async Task ProcessBooking(Booking booking, CancellationToken cancellation)
    {
        var dbContext = _serviceScope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var temp = await dbContext.Bookings
            .Where(x => x.Id == booking.Id && x.Status == BookingStatus.Pending)
            .GroupJoin(dbContext.Events, b => b.EventId, e => e.Id, (
                booking, events) => new
                {
                    Booking = booking,
                    Event = events.SingleOrDefault()
                })
            .SingleOrDefaultAsync(cancellation);

        if (temp is { Booking: Booking destBooking })
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellation);

            destBooking.Status = temp is { Event: Event }
                ? BookingStatus.Confirmed
                : BookingStatus.Rejected;
            destBooking.ProcessedAt = DateTime.Now;

            await dbContext.SaveChangesAsync(cancellation);

            logger.LogInformation("Бронь {Booking} для события {Event} обработана.", destBooking.Id, destBooking.EventId);
        }
    }
}