using BookingService.Application;
using BookingService.Domain;
using Common.Contracts.Kafka;
using Common.Kafka;

namespace BookingService.Kafka.Services;

public class EventsServiceNotifier(IEventPublisher publisher) : IEventsServiceNotifier
{
    public Task BookingConfirmedAsync(Booking booking, Guid? userId, CancellationToken cancellation)
        => publisher.PublishAsync(
            Topics.BookingAction, 
            new BookingActionMessage(
                booking.Id, booking.EventId, 1, 
                userId ?? booking.UserId, booking.CreatedAt, BookingAction.Confirm), 
            () => booking.EventId.ToString(),
            cancellation);

    public Task BookingCancelledAsync(Booking booking, Guid? userId, CancellationToken cancellation)
        => publisher.PublishAsync(
            Topics.BookingAction,
            new BookingActionMessage(
                booking.Id, booking.EventId, 1,  
                userId ?? booking.UserId, booking.CreatedAt, BookingAction.Cancel),
            () => booking.EventId.ToString(),
            cancellation);
}
