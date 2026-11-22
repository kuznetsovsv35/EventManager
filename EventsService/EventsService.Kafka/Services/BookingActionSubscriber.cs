using Common.Contracts.Kafka;
using Common.Kafka;
using EventsService.Application;
using EventsService.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventsService.Kafka.Services;

public class BookingActionSubscriber(
    IServiceScopeFactory scopeFactory,
    ILogger<BookingActionSubscriber> logger) : IEventSubscriber<BookingActionMessage>
{
    public async Task HandleMessage(BookingActionMessage message, CancellationToken cancellation)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var events = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        await events.ExecuteEventBookingAsync(message.BookingId, message.EventId, (eventBooking) =>
        {
            if (eventBooking.IsProcessed)
                return false;
            
            eventBooking.UserId = message.UserId;

            switch(message.Action)
            {
                case BookingAction.Confirm:
                    HandleConfirmed(eventBooking, message.Seats);
                    break;
                case BookingAction.Cancel:
                    HandleCancelled(eventBooking);
                    break;
                default:
                    return false;
            }
            return true;
        }, cancellation);
    }

    void HandleConfirmed(EventBooking eventBooking, int seats)
    {
        if (eventBooking.IsEventValid)
        {
            var confirmResult = eventBooking.Confirm(seats);

            if (confirmResult == false)
                logger.LogWarning("На событии {eventId} нет достаточно свободных мест", eventBooking.EventId);
        }
        else
        {
            eventBooking.Cancel();
            logger.LogWarning("Событие {eventId} не найдено", eventBooking.EventId);
        }
    }

    static void HandleCancelled(EventBooking eventBooking) => eventBooking.Cancel();
}
