using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Tests;

public class BookingServiceTest : TraitAttributes
{
    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task CreateBookingExistingEvent_Success()
    {
        // Given
        var ctx = new BookingServiceTestContext();
        using var scope = ctx.CreateScope();
        var eventId = await ctx.GetRandomEventId(CancellationToken.None);
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        // When
        var bookingInfo = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        var booking = await db.Bookings.SingleAsync(x => x.Id == bookingInfo.Id);

        // Then
        Assert.Equal(eventId, bookingInfo.EventId);
        Assert.Equal(eventId, booking.EventId);

        Assert.Equal(BookingStatus.Pending, bookingInfo.Status);
        Assert.Equal(BookingStatus.Pending, booking.Status);

        Assert.Null(bookingInfo.ProcessedAt);
        Assert.Null(booking.ProcessedAt);
    }

    [Trait(Category, Category_Booking)]
    [Theory]
    [InlineData([10])]
    [InlineData([20])]
    [InlineData([50])]
    public async Task CreateMultiBookingsForEvent_Success(int bookingCount)
    {
        // Given
        var ctx = new BookingServiceTestContext();
        using var scope = ctx.CreateScope();
        var eventId = await ctx.GetRandomEventId(CancellationToken.None);
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        // When
        var bookingIds = Enumerable
            .Range(0, bookingCount)
            .Select(async _ => (await bookingService.CreateBookingAsync(eventId, CancellationToken.None)).Id)
            .Select(t => t.Result)
            .ToList();

        // Then
        Assert.Equal(bookingCount, bookingIds.Count);
        Assert.Distinct(bookingIds);
    }

    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task GetBookingById_Success()
    {
        // Given
        var ctx = new BookingServiceTestContext();
        using var scope = ctx.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventId = await ctx.GetRandomEventId(CancellationToken.None);
        var booking = new Booking()
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now,
        };

        // When
        var bookingCreated = booking.ToInfo();
        await db.AddBookingAsync(booking, CancellationToken.None);
        var bookingFound = await bookingService.GetBookingByIdAsync(booking.Id, CancellationToken.None);

        // Then
        Assert.Equal(bookingCreated.Id, bookingFound.Id);
        Assert.Equal(eventId, bookingCreated.EventId);
        Assert.Equal(bookingCreated.EventId, bookingFound.EventId);
        Assert.Equal(BookingStatus.Pending, bookingCreated.Status);
        Assert.Equal(bookingCreated.Status, bookingFound.Status);
        Assert.Null(bookingCreated.ProcessedAt);
        Assert.Null(bookingFound.ProcessedAt);
    }

    [Fact]
    public async Task TestBookingQueue_Success()
    {
        // Given
        var ctx = new BookingServiceTestContext();
        var queue = ctx.ServiceProvider.GetRequiredService<IAsyncQueue<Booking>>();
        var queuedBooking = new Booking()
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now,
        };

        // When
        var queueTask = queue.Dequeue(CancellationToken.None);
        await queue.Enqueue(queuedBooking);
        var dequeuedBooking = await queueTask;

        // Then
        Assert.NotNull(dequeuedBooking);
        Assert.Equal(queuedBooking, dequeuedBooking);
    }

    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task TestRunStopBackgroundService_Success()
    {
        // Given
        var ctx = new BookingServiceTestContext();
        var service = ctx.ServiceProvider.GetRequiredService<IAppBackgroundService>();

        // When
        var statusBeforeStart = service.Status;
        await service.StartAsync(CancellationToken.None);
        var statusAfterStart = service.Status;

        await Task.Delay(TimeSpan.FromSeconds(2));
        var statusRunning = service.Status;

        await service.StopAsync(CancellationToken.None);
        var statusAfterStop = service.Status;

        await Task.Delay(TimeSpan.FromSeconds(2));
        var statusStopped = service.Status;

        // Then
        Assert.Equal(BackgroundServiceStatus.Stopped, statusBeforeStart);
        Assert.NotEqual(BackgroundServiceStatus.Stopped, statusAfterStart);
        Assert.Equal(BackgroundServiceStatus.Running, statusRunning);
        Assert.NotEqual(BackgroundServiceStatus.Running, statusAfterStop);
        Assert.Equal(BackgroundServiceStatus.Stopped, statusStopped);
    }

    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task TestChangeStatusOneEvent_Success()
    {
        // Given        
        var ctx = new BookingServiceTestContext();
        using var scope = ctx.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var backService = ctx.ServiceProvider.GetRequiredService<IAppBackgroundService>();
        var eventId = await ctx.GetRandomEventId(CancellationToken.None);

        // When
        var bookingCreated = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        var bookingBeforeChange = await bookingService.GetBookingByIdAsync(bookingCreated.Id, CancellationToken.None);

        await backService.StartAsync(CancellationToken.None);

        var bookingBeforeChange2 = await bookingService.GetBookingByIdAsync(bookingCreated.Id, CancellationToken.None);

        await Task.Delay(TimeSpan.FromSeconds(3));

        BookingInfo? bookingAfterChange;
        using (IServiceScope scope2 = ctx.CreateScope())
        {
            bookingAfterChange = await scope2.ServiceProvider
                .GetRequiredService<IBookingService>()
                .GetBookingByIdAsync(bookingCreated.Id, CancellationToken.None);
        }

        await backService.StopAsync(CancellationToken.None);

        // Then
        Assert.Equal(eventId, bookingCreated.EventId);
        Assert.Equal(eventId, bookingBeforeChange.EventId);
        Assert.Equal(eventId, bookingBeforeChange2.EventId);
        Assert.Equal(eventId, bookingAfterChange.EventId);

        Assert.Equal(BookingStatus.Pending, bookingCreated.Status);
        Assert.Equal(BookingStatus.Pending, bookingBeforeChange.Status);
        Assert.Equal(BookingStatus.Pending, bookingBeforeChange2.Status);
        Assert.NotEqual(BookingStatus.Pending, bookingAfterChange.Status);

        Assert.Null(bookingCreated.ProcessedAt);
        Assert.Null(bookingBeforeChange.ProcessedAt);
        Assert.Null(bookingBeforeChange2.ProcessedAt);
        Assert.NotNull(bookingAfterChange.ProcessedAt);
    }

    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task CreateBookingNotExistEvent_Fail()
    {
        // Given
        var ctx = new BookingServiceTestContext();
        using var scope = ctx.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventId = Guid.NewGuid();

        // Then
        await Assert.ThrowsAsync<EventNotFoundException>(() => bookingService.CreateBookingAsync(eventId, CancellationToken.None));
    }

    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task CreateBookingForDeletedEvent_Fail()
    {
        // Given
        var ctx = new BookingServiceTestContext();
        using var scope = ctx.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventId = await ctx.GetRandomEventId(CancellationToken.None);

        // When
        var deletingEvent = await db.Events.SingleAsync(x => x.Id == eventId);
        db.DeleteEvent(deletingEvent);
        var deletedEvent = await db.Events.SingleOrDefaultAsync(x => x.Id == eventId);

        // Then
        Assert.Equal(eventId, deletingEvent.Id);
        Assert.Null(deletedEvent);
        await Assert.ThrowsAsync<EventNotFoundException>(() => bookingService.CreateBookingAsync(eventId, CancellationToken.None));
    }

    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task GetBookingById_Fail()
    {
        // Given
        var ctx = new BookingServiceTestContext();
        using var scope = ctx.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var bookingId = Guid.NewGuid();

        // Then
        await Assert.ThrowsAsync<BookingNotFoundException>(() => bookingService.GetBookingByIdAsync(bookingId, CancellationToken.None));
    }
}