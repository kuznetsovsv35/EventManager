using BookingService.Application;
using BookingService.Domain;
using Common.Contracts.Kafka;
using Common.Kafka;

namespace BookingService.Kafka.Services;

public class EventsServiceNotifier(IEventPublisher publisher) : IEventsServiceNotifier
{
    public Task BookingConfirmedAsync(Booking booking, CancellationToken cancellation)
        => publisher.PublishAsync(
            Topics.BookingConfirmed, 
            new BookingConfirmed(booking.Id, booking.EventId, 1, booking.UserId, booking.ProcessedAt!.Value), 
            () => booking.EventId.ToString(),
            cancellation);
}
