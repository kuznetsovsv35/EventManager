using Common.Contracts.Kafka;
using Common.Kafka;
using Common.Tests;
using EventsService.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventsService.UnitTests;

public class EventBookingTest(TestContext context) : IClassFixture<TestContext>
{
    [Trait(Category.Name, Category.Service)]
    [Fact]
    public async Task BookingConfirmed_Success()
    {
        // Given
        var services = context.CreateServiceProvider();
        var eventId = await services.GetRandomEventId(CancellationToken.None);
        await using var work = services.CreateAsyncScope();
        var workDb = work.ServiceProvider.GetRequiredService<EventsDbContext>();
        var @event = await workDb.Events.AsNoTracking().SingleAsync(x => x.Id == eventId);
        var seats = @event.AvailableSeats;
        var bookedAt = @event.StartAt.AddDays(-1);
        var subscriber = services.GetRequiredService<IEventSubscriber<BookingActionMessage>>();
        var bookingId = Guid.NewGuid();

        // When
        BookingActionMessage message = new(bookingId, eventId, seats, Guid.Empty, bookedAt, BookingAction.Confirm);
        await subscriber.HandleMessage(message, CancellationToken.None);

        await using var verify = services.CreateAsyncScope();
        var verifyDb = verify.ServiceProvider.GetRequiredService<EventsDbContext>();
        var actualSeats = (await verifyDb.Events.AsNoTracking().SingleAsync(x => x.Id == eventId)).AvailableSeats;
        var booking = await verifyDb.EventBookings
            .AsNoTracking()
            .Include(x => x.Event)
            .SingleAsync(x => x.BookingId == bookingId);

        // Then
        Assert.Equal(eventId, booking.EventId);
        Assert.True(booking.IsEventValid);
        Assert.Equal(0, actualSeats);
        Assert.True(booking.IsProcessed);
        Assert.True(booking.IsConfirmed);
        Assert.False(booking.IsCancelled);
    }

    [Trait(Category.Name, Category.Service)]
    [Fact]
    public async Task BookingCancelled_Success()
    {
        // Given
        var services = context.CreateServiceProvider();
        var eventId = await services.GetRandomEventId(CancellationToken.None);
        await using var work = services.CreateAsyncScope();
        var workDb = work.ServiceProvider.GetRequiredService<EventsDbContext>();
        var @event = await workDb.Events.AsNoTracking().SingleAsync(x => x.Id == eventId);
        var seats = @event.AvailableSeats;
        var bookedAt = @event.StartAt.AddDays(-1);
        var subscriber = services.GetRequiredService<IEventSubscriber<BookingActionMessage>>();
        var bookingId = Guid.NewGuid();

        // When
        BookingActionMessage confirmMessage = new(bookingId, eventId, seats, Guid.Empty, bookedAt, BookingAction.Confirm);
        await subscriber.HandleMessage(confirmMessage, CancellationToken.None);

        BookingActionMessage cancelMessage = new(bookingId, eventId, seats, Guid.Empty, bookedAt, BookingAction.Cancel);
        await subscriber.HandleMessage(cancelMessage, CancellationToken.None);

        await using var verify = services.CreateAsyncScope();
        var verifyDb = verify.ServiceProvider.GetRequiredService<EventsDbContext>();
        var actualSeats = (await verifyDb.Events.AsNoTracking().SingleAsync(x => x.Id == eventId)).AvailableSeats;
        var booking = await verifyDb.EventBookings
            .AsNoTracking()
            .Include(x => x.Event)
            .SingleAsync(x => x.BookingId == bookingId);

        // Then
        Assert.Equal(eventId, booking.EventId);
        Assert.True(booking.IsEventValid);
        Assert.Equal(seats, actualSeats);
        Assert.True(booking.IsProcessed);
        Assert.True(booking.IsConfirmed);
        Assert.True(booking.IsCancelled);
    }

    [Trait(Category.Name, Category.Service)]
    [Fact]
    public async Task BookingConfirmedNonExistingEvent_Fail()
    {
        // Given
        var services = context.CreateServiceProvider();
        var eventId = Guid.NewGuid();
        var bookedAt = TestDbContext.StartAt.AddDays(-1);
        var subscriber = services.GetRequiredService<IEventSubscriber<BookingActionMessage>>();
        var bookingId = Guid.NewGuid();

        // When
        BookingActionMessage message = new(bookingId, eventId, 1, Guid.Empty, bookedAt, BookingAction.Confirm);
        await subscriber.HandleMessage(message, CancellationToken.None);

        await using var verify = services.CreateAsyncScope();
        var verifyDb = verify.ServiceProvider.GetRequiredService<EventsDbContext>();
        var booking = await verifyDb.EventBookings
            .AsNoTracking()
            .SingleAsync(x => x.BookingId == bookingId);

        // Then
        Assert.Null(booking.EventId);
        Assert.True(booking.IsProcessed);
        Assert.False(booking.IsConfirmed);
        Assert.True(booking.IsCancelled);
    }

    [Trait(Category.Name, Category.Service)]
    [Fact]
    public async Task BookingConfirmedPastEvent_Fail()
    {
        // Given
        var services = context.CreateServiceProvider();
        var eventId = await services.GetRandomEventId(CancellationToken.None);
        await using var work = services.CreateAsyncScope();
        var workDb = work.ServiceProvider.GetRequiredService<EventsDbContext>();
        var @event = await workDb.Events.AsNoTracking().SingleAsync(x => x.Id == eventId);
        var seats = @event.AvailableSeats;
        var bookedAt = @event.StartAt.AddDays(+1);
        var subscriber = services.GetRequiredService<IEventSubscriber<BookingActionMessage>>();
        var bookingId = Guid.NewGuid();

        // When
        BookingActionMessage message = new(bookingId, eventId, seats, Guid.Empty, bookedAt, BookingAction.Confirm);
        await subscriber.HandleMessage(message, CancellationToken.None);

        await using var verify = services.CreateAsyncScope();
        var verifyDb = verify.ServiceProvider.GetRequiredService<EventsDbContext>();
        var actualSeats = (await verifyDb.Events.AsNoTracking().SingleAsync(x => x.Id == eventId)).AvailableSeats;
        var booking = await verifyDb.EventBookings
            .AsNoTracking()
            .Include(x => x.Event)
            .SingleAsync(x => x.BookingId == bookingId);

        // Then
        Assert.Equal(eventId, booking.EventId);
        Assert.True(booking.IsEventValid);
        Assert.Equal(seats, actualSeats);
        Assert.True(booking.IsProcessed);
        Assert.False(booking.IsConfirmed);
        Assert.True(booking.IsCancelled);
    }

    [Trait(Category.Name, Category.Service)]
    [Fact]
    public async Task BookingConfirmedNoAvailableSeats_Fail()
    {
        // Given
        var services = context.CreateServiceProvider();
        var eventId = await services.GetRandomEventId(CancellationToken.None);
        await using var work = services.CreateAsyncScope();
        var workDb = work.ServiceProvider.GetRequiredService<EventsDbContext>();
        var @event = await workDb.Events.AsNoTracking().SingleAsync(x => x.Id == eventId);
        var seats = @event.AvailableSeats;
        var bookedAt = @event.StartAt.AddDays(-1);
        var subscriber = services.GetRequiredService<IEventSubscriber<BookingActionMessage>>();
        var bookingId = Guid.NewGuid();

        // When
        BookingActionMessage message = new(bookingId, eventId, seats + 1, Guid.Empty, bookedAt, BookingAction.Confirm);
        await subscriber.HandleMessage(message, CancellationToken.None);

        await using var verify = services.CreateAsyncScope();
        var verifyDb = verify.ServiceProvider.GetRequiredService<EventsDbContext>();
        var actualSeats = (await verifyDb.Events.AsNoTracking().SingleAsync(x => x.Id == eventId)).AvailableSeats;
        var booking = await verifyDb.EventBookings
            .AsNoTracking()
            .Include(x => x.Event)
            .SingleAsync(x => x.BookingId == bookingId);

        // Then
        Assert.Equal(eventId, booking.EventId);
        Assert.True(booking.IsEventValid);
        Assert.Equal(seats, actualSeats);
        Assert.True(booking.IsProcessed);
        Assert.False(booking.IsConfirmed);
        Assert.True(booking.IsCancelled);
    }
}
