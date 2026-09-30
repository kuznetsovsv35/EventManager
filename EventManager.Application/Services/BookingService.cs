using EventManager.Domain.ValueObjects;
using EventManager.Domain.Exceptions;
using EventManager.Common.Interfaces;
using EventManager.Application.Interfaces;
using EventManager.Application.DataTransferObjects;

namespace EventManager.Application.Services;

public class BookingService(
    ISyncContextFactory syncContextFactory,
    IBookingRepository bookings,
    IEventRepository events,
    IBookingServiceNotifier notifier) : IBookingService
{
    public Task<BookingInfo> CreateBookingAsync(
        Guid eventId, 
        CancellationToken cancellation)
        => CreateBookingAsync(eventId, new(){ Id = Guid.Empty }, cancellation);

    public async Task<BookingInfo> CreateBookingAsync(
        Guid eventId, 
        UserInfo userInfo,
        CancellationToken cancellation)
    {
        await CheckUserActiveBookingsAsync(userInfo, cancellation);

        var booking = await syncContextFactory.CreateContext<Booking>().ExecuteActionAsync<Booking>(async () =>
        {
            if (await events.GetEventAsync(eventId, cancellation) is Event @event)
            {
                cancellation.ThrowIfCancellationRequested();

                if (DateTime.UtcNow > @event.StartAt)
                    throw new BookingStartedException(eventId);

                if (@event.TryReserveSeats())
                {
                    var booking = new Booking(eventId, userInfo.Id);
                    await bookings.AddBookingAsync(booking, cancellation);
                    await events.UpdateEventAsync(@event, cancellation);
                    return booking;
                }
                throw new NoAvailableSeatsException(eventId);
            }
            throw new EventNotFoundException(eventId, nameof(eventId));
        }, cancellation);

        await notifier.BookingCreatedAsync(booking.Id, cancellation);
        return booking.ToInfo();
    }

    public async Task<BookingInfo> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellation)
    {
        if (await bookings.GetBookingAsync(bookingId, cancellation) is Booking booking)
            return booking.ToInfo();

        throw new BookingNotFoundException(bookingId, nameof(bookingId));
    }

    public async Task<BookingInfo> CancelBookingAsync(Guid bookingId, UserInfo userInfo, CancellationToken cancellation)
    {
        if (await bookings.GetBookingAsync(bookingId, cancellation) is not Booking booking)
            throw new BookingNotFoundException(bookingId, nameof(bookingId));
        
        CheckUserRole(booking, userInfo);

        if (!booking.Cancel())
            throw new InvalidBookingOperationException(booking.Id, booking.Status);
        
        await bookings.UpdateBookingStatusAsync(booking, cancellation);
        return booking.ToInfo();
    }

    async Task CheckUserActiveBookingsAsync(UserInfo userInfo, CancellationToken cancellation)
    {
        if (await bookings.GetUserActiveBookingCountAsync(userInfo.Id) is int activeBookings && activeBookings > AavailableBookingsPerUser)
            throw new UserBookingLimitException(userInfo.Login, activeBookings, AavailableBookingsPerUser);
    }

    void CheckUserRole(Booking booking, UserInfo userInfo)
    {
        if (userInfo.Role == UserRole.Admin || booking.UserId == userInfo.Id)
            return;

        throw new AccessDeniedException(userInfo.Login);
    }
}