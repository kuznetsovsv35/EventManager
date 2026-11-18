using Common.Contracts.Kafka;
using Common.Kafka;
using EventsService.Application;
using EventsService.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace EventsService.Kafka.Services;

public class BookingConfirmSubscriber(IServiceScopeFactory scopeFactory) : IEventSubscriber<BookingConfirmed>
{
    public async Task HandleMessage(BookingConfirmed message, CancellationToken cancellation)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var events = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        if (await events.GetEventAsync(message.EventId, cancellation) is Event @event)
        {
            @event.ReleaseSeats(message.ReservedSeats);
            await events.UpdateEventAsync(@event, cancellation);
        }
    }
}
