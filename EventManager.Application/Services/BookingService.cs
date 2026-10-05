using EventManager.Domain.ValueObjects;
using EventManager.Domain.Exceptions;
using EventManager.Common.Interfaces;
using EventManager.Application.Interfaces;
using EventManager.Application.DataTransferObjects;
using EventManager.Application.Authorization;

namespace EventManager.Application.Services;

public class BookingService(
    ISyncContextFactory syncContextFactory,
    IBookingRepository bookings,
    IEventRepository events,
    IBookingServiceNotifier notifier) : IBookingService
{
    const int AvailableBookingsPerUser = 10;

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
                    throw new StartedEventBookingException(@event, userInfo.Login, userInfo.Role);

                if (@event.TryReserveSeats())
                {
                    var booking = new Booking(eventId, userInfo.Id);
                    await bookings.AddBookingAsync(booking, cancellation);
                    await events.UpdateEventAsync(@event, cancellation);
                    return booking;
                }
                throw new NoAvailableSeatsException(@event, userInfo.Login, userInfo.Role);
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
            throw new InvalidBookingOperation("Неверный статус брони для операции отмены", booking, userInfo.Login, userInfo.Role);
        
        await bookings.UpdateBookingStatusAsync(booking, cancellation);
        return booking.ToInfo();
    }

    async Task CheckUserActiveBookingsAsync(UserInfo userInfo, CancellationToken cancellation)
    {
        if (await bookings.GetUserActiveBookingCountAsync(userInfo.Id, cancellation) is int activeBookings && activeBookings > AvailableBookingsPerUser)
            throw new UserOperationException("Превышение максимального числа активных броней для одного пользователя", userInfo.Login, userInfo.Role);
    }

    void CheckUserRole(Booking booking, UserInfo userInfo)
    {
        if (userInfo.Role == UserRole.Admin || booking.UserId == userInfo.Id)
            return;

        throw new ForbiddenException<BookingService>(Policies.BookingService.CancelBooking, userInfo.Login, userInfo.Role);
    }
}