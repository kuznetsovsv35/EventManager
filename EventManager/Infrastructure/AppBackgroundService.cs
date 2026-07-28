using EventManager.Application.Interfaces;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure;

public class AppBackgroundService(
    IServiceScopeFactory scopeFactory,
    IAsyncQueue<Booking> bookingQueue,
    ILogger<AppBackgroundService> logger) : BackgroundService, IAppBackgroundService
{
    /// <summary>
    /// Имитация обработки брони.
    /// </summary>
    static readonly TimeSpan ProcessingDelay = TimeSpan.FromSeconds(2);
    /// <summary>
    /// Пауза восстановления сервиса после исключения.
    /// </summary>
    static readonly TimeSpan RecoveryPause = TimeSpan.FromSeconds(1);

    BackgroundServiceStatus _status = BackgroundServiceStatus.Stopped;

    public BackgroundServiceStatus Status => _status;

    public event EventHandler<Booking>? ProcessBooking;

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
                        await Task.Delay(RecoveryPause, stoppingToken);
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
                Booking = booking,
                Event = events.FirstOrDefault(),
            })
            .FirstOrDefaultAsync(cancellation);

        if (temp is { Booking: Booking })
        {         
            try
            {
                await CustomProcessBooking(booking, cancellation);
                if (temp is { Event: null })
                    throw new EventNotFoundException(nameof(booking.EventId), booking.EventId);
            }
            catch(Exception ex) when (ex is not OperationCanceledException)
            {
                booking.Reject();
                logger.LogError(ex, "Ошибка обработки брони {Booking} для события {Event}.", booking.Id, booking.EventId);
            }

            await UpdateData(dbContext, booking, cancellation);

            switch (booking.Status)
            {
                case BookingStatus.Confirmed:
                    logger.LogInformation("Бронь {Booking} для события {Event} подтверждена.", booking.Id, booking.EventId);
                    break;
                case BookingStatus.Rejected:
                    logger.LogWarning("Бронь {Booking} для события {Event} отклонена.", booking.Id, booking.EventId);
                    break;
                default:
                    throw new InvalidDataException($"Неверный статус брони {booking.Id}.");
            }
        }
        else
            logger.LogError("Бронь {Booking} для события {Event} в БД не найдена.", booking.Id, booking.EventId);
    }

    static Task UpdateData(IAppDbContext dbContext, Booking booking, CancellationToken cancellation)
        => dbContext.CreateSyncContext<Booking>().ExecuteActionAsync(async() =>
        {
            if (booking.Status == BookingStatus.Rejected)
            {
                if (await dbContext.Events.FindAsync(booking.EventId, cancellation) is Event @event)
                    @event.ReleaseSeats();
            }
            dbContext.Bookings.Update(booking);
        }, cancellation);

    Task CustomProcessBooking(Booking booking, CancellationToken cancellation)
    {
        ProcessBooking?.Invoke(this, booking);
        if (booking.Status == BookingStatus.Pending)
            booking.Confirm();
        return Task.Delay(ProcessingDelay, cancellation);
    }
}