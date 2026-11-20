using Common.Contracts.Kafka;
using Common.Kafka;
using EventsService.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventsService.Kafka.Services;

public class BookingConfirmSubscriber(
    IServiceScopeFactory scopeFactory,
    ILogger<BookingConfirmSubscriber> logger) : IEventSubscriber<BookingConfirmed>
{
    public async Task HandleMessage(BookingConfirmed message, CancellationToken cancellation)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var events = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        await events.ExecuteEventBookingAsync(message.BookingId, message.EventId, (eventBooking) => 
        {
            if (!eventBooking.IsProcessed)
            {                
                eventBooking.UserId = message.UserId;

                if (!eventBooking.IsEventValid)
                    logger.LogWarning("Событие {eventId} не найдено", message.EventId);

                var confirmResult = eventBooking.Confirm(message.ReservedSeats);
                
                if (confirmResult == false)
                    ;

                return true;
            }
            return false;
        }, cancellation);
    }
}
