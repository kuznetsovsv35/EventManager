using EventManager.Application.Interfaces;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure;

public class AppBackgroundService(
    IServiceScopeFactory scopeFactory, 
    IAsyncQueue<Booking> bookingQueue,
    ILogger<AppBackgroundService> logger) : BackgroundService
{
    IAppDbContext? _dbContext;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Старт фонового процесса обработки ...");
        try
        {
            _dbContext = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IAppDbContext>();
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessBooking(await bookingQueue.Dequeue(stoppingToken), stoppingToken);
                }
                catch (OperationCanceledException canceled ) when(canceled.CancellationToken.IsCancellationRequested)
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
            logger.LogInformation("Завершение фонового процесса обработки...");
        }
    }

    async Task ProcessBooking(Booking booking, CancellationToken cancellation)
    {
        if (_dbContext == null)
            return;

        var temp =  await _dbContext.Bookings
            .Where(x => x.Id == booking.Id && x.Status == BookingStatus.Pending)
            .GroupJoin(_dbContext.Events, b => b.EventId, e => e.Id, (
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

            await _dbContext.UpdateBookingAsync(destBooking, cancellation);

            logger.LogInformation("Бронь {Booking} для события {Event} обработана.", destBooking.Id, destBooking.EventId);
        }
    }
}