using Authorization.Domain;
using Authorization;
using Authorization.Exceptions;
using BookingService.Application.DTO;
using BookingService.Application.Security;
using BookingService.Domain;
using BookingService.Domain.Exceptions;
using Common.Synchronization;

namespace BookingService.Application.Services;

public class BookingService(
    IAppAuthorizationService appAuthorization,
    ISyncContextFactory syncContextFactory,
    IBookingRepository bookings,
    IBookingServiceNotifier bookingServiceNotifier,
    IEventsServiceNotifier eventsServiceNotifier) : IBookingService
{
    const int AvailableBookingsPerUser = 10;

    public async Task<BookingInfo> CreateBookingAsync(Guid eventId, CancellationToken cancellation)
    {
        await appAuthorization.AuthorizeAsync(this, Policy.CreateBooking, cancellation);
        var booking = await syncContextFactory.CreateContext<Booking>().ExecuteActionAsync<Booking>(async () =>
        {
            await CheckUserActiveBookingsAsync(cancellation);

            cancellation.ThrowIfCancellationRequested();

            var booking = new Booking(eventId, appAuthorization.CurrentUser.Id);
            await bookings.AddBookingAsync(booking, cancellation);
            return booking;
        }, cancellation);

        await bookingServiceNotifier.BookingCreatedAsync(booking.Id, cancellation);
        return booking.ToInfo();
    }

    public async Task<BookingInfo> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellation)
    {
        await appAuthorization.AuthorizeAsync(this, Policy.GetBooking, cancellation);

        if (await bookings.GetBookingAsync(bookingId, cancellation) is Booking booking)
            return booking.ToInfo();

        throw new BookingNotFoundException(bookingId, nameof(bookingId));
    }

    public async Task<BookingInfo> CancelBookingAsync(Guid bookingId, CancellationToken cancellation)
    {
        await appAuthorization.AuthorizeAsync(this, Policy.CancelBooking, cancellation);

        if (await bookings.GetBookingAsync(bookingId, cancellation) is not Booking booking)
            throw new BookingNotFoundException(bookingId, nameof(bookingId));

        CheckUserRole(booking);

        if (!booking.Cancel())
            throw new InvalidBookingOperationException("Неверный статус брони для операции отмены", booking, appAuthorization.CurrentUser);

        await bookings.UpdateBookingStatusAsync(booking, cancellation);
        await eventsServiceNotifier.BookingCancelledAsync(booking, appAuthorization.CurrentUser.Id, cancellation);
        return booking.ToInfo();
    }

    async Task CheckUserActiveBookingsAsync(CancellationToken cancellation)
    {
        if (await bookings.GetUserActiveBookingCountAsync(appAuthorization.CurrentUser.Id, cancellation) is int activeBookings && activeBookings >= AvailableBookingsPerUser)
            throw new ActiveUserBookingException(appAuthorization.CurrentUser);
    }

    void CheckUserRole(Booking booking)
    {
        if (appAuthorization.CurrentUser.Role == UserRole.Admin || booking.UserId == appAuthorization.CurrentUser.Id)
            return;

        throw new ForbiddenException(this, Policy.CancelBooking, appAuthorization.CurrentUser);
    }
}