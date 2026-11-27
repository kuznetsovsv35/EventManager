using Common.Contracts.Kafka;
using Common.Kafka;
using EventsService.Application;
using EventsService.Domain.Exceptions;
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
        await events.ExecuteEventBookingAsync(message.BookingId, message.EventId, message.UserId, (eventBooking) =>
        {
            if (
                message.Action switch
                {
                    BookingAction.Confirm => eventBooking.IsProcessed,
                    BookingAction.Cancel => eventBooking.IsCancelled,
                    _ => true
                }) return false;

            try
            {
                switch (message.Action)
                {
                    case BookingAction.Confirm:
                        eventBooking.Confirm(message.Seats, message.CreatedAt);
                        break;
                    case BookingAction.Cancel:
                        eventBooking.Cancel();
                        break;
                    default:
                        return false;
                }
            }
            catch (InvalidEventOperationException ex)
            {
                eventBooking.Cancel();
                logger.LogError(ex.Message);
            }

            return true;
        }, cancellation);
    }
}
