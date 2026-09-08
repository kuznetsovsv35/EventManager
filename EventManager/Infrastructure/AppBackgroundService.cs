using System.Threading.Channels;
using EventManager.Application.Interfaces;
using EventManager.Models;

namespace EventManager.Infrastructure;

public class AppBackgroundService(
    IServiceScopeFactory scopeFactory,
    ISyncContextFactory syncContextFactory,
    Channel<Guid> triggerChannel,
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
    /// <summary>
    /// Размер пакета для параллельной обработки.
    /// </summary>
    const int ChunkSize = 50;
    /// <summary>
    /// Интервал опроса.
    /// </summary>
    static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

    readonly ChannelReader<Guid> _triggerReader = triggerChannel.Reader;

    BackgroundServiceStatus _status = BackgroundServiceStatus.Stopped;

    int _isProcessing;

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
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                cts.CancelAfter(PollingInterval);
                try
                {
                    if (await _triggerReader.WaitToReadAsync(cts.Token))
                    {
                        while (_triggerReader.TryRead(out var _))
                        {
                            stoppingToken.ThrowIfCancellationRequested();
                            await ProcessBookingsAsync(stoppingToken);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    else
                    {
                        cts.TryReset();
                        await ProcessBookingsAsync(stoppingToken);                        
                    }
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

    async Task ProcessBookingsAsync(CancellationToken cancellation)
    {
        if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) != 0)
            return;

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            await foreach (var chunk in bookingRepo.GetPendingBookingsAsync(ChunkSize, cancellation))
            {
                await Task.WhenAll(chunk.Select(async booking =>
                {
                    await ProcessBookingAsync(booking, cancellation);
                }));
            }
        }
        finally
        {
            Interlocked.Exchange(ref _isProcessing, 0);
        }
    }

    async Task ProcessBookingAsync(Booking booking, CancellationToken cancellation)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var bookings = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

        try
        {
            await CustomProcessBooking(booking, cancellation);
            if (booking is { Event: null })
                throw new EventNotFoundException(booking.EventId, nameof(booking.EventId));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            booking.Reject();
            logger.LogError(ex, "Ошибка обработки брони {Booking} для события {Event}.", booking.Id, booking.EventId);
        }

        await syncContextFactory.CreateContext<Booking>().ExecuteActionAsync(async ()
            => await bookings.UpdateBookingStatusAsync(booking, cancellation), cancellation);

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

    Task CustomProcessBooking(Booking booking, CancellationToken cancellation)
    {
        ProcessBooking?.Invoke(this, booking);
        if (booking.Status == BookingStatus.Pending)
            booking.Confirm();
        return Task.Delay(ProcessingDelay, cancellation);
    }
}