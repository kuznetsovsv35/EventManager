using EventManager.Application.Interfaces;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure;

public class AppBackgroundService(
    IServiceScopeFactory scopeFactory,
    IAsyncQueue<Booking> bookingQueue,
    ILogger<AppBackgroundService> logger) : BackgroundService, IAppBackgroundService
{
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
        Interlocked.Exchange(ref _status, BackgroundServiceStatus.Running);
        logger.LogInformation("Старт фонового процесса обработки ...");
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var tasks = (await bookingQueue.DequeueAll(stoppingToken))
                        .Select(booking => ProcessBookingAsync(booking, stoppingToken));
                    await Task.WhenAll(tasks); 
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

    async Task ProcessBookingAsync(Booking booking, CancellationToken cancellation)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var temp = await dbContext.GetBookings(x => x.Id == booking.Id && x.Status == BookingStatus.Pending)
            .GroupJoin(dbContext.GetEvents(), b => b.EventId, e => e.Id, (booking, events) => new
            {
                booking.Status,
                EventId = GetFirstId(events),
            })
            .FirstOrDefaultAsync(cancellation);

        if (temp is { Status: BookingStatus.Pending })
        {         
            await Task.Delay(TimeSpan.FromSeconds(2), cancellation);
            var bookingConfirmed = true;
            
            if (temp is { EventId: Guid eventId })
            {                                
                if (!bookingConfirmed)
                {
                    await dbContext.CreateSyncContext<Event>().ExecuteActionAsync(async() =>
                    {
                        if (await dbContext.Events.FindAsync(eventId) is Event dest)
                            dest.ReleaseSeats();
                    }, CancellationToken.None);
                }
            }
            else
            {
                bookingConfirmed = false;
            }

            await dbContext.CreateSyncContext<Booking>().ExecuteActionAsync(async() =>
            {
                if (await dbContext.Bookings.FindAsync(booking.Id) is Booking dest)
                {
                    if (bookingConfirmed)
                        dest.Confirm();
                    else
                    {
                        dest.Reject();
                    }
                }
            }, CancellationToken.None);

            if (bookingConfirmed)
                logger.LogInformation("Бронь {Booking} для события {Event} подтверждена.", booking.Id, booking.EventId);
            else
                logger.LogWarning("Бронь {Booking} для события {Event} отклонена.", booking.Id, booking.EventId);
        }
        else
            logger.LogError("Бронь {Booking} для события {Event} в БД не найдена.", booking.Id, booking.EventId);
    }
    static Guid? GetFirstId(IEnumerable<Event> events)
    {
        if (events.FirstOrDefault() is Event @event)
            return @event.Id;
        return null;
    }
}