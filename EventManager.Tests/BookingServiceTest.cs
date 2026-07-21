using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure;
using EventManager.Models;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Tests;

/// <summary>
/// Тесты сервиса бронирования.
/// </summary>
public class BookingServiceTest(EventManagerTestContext context) : TraitAttributes, IClassFixture<EventManagerTestContext>
{
    /// <summary>
    /// Создание брони для существующего события.
    /// </summary>
    /// <returns></returns>
    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task CreateBookingExistingEvent_Success()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var eventId = await context.GetRandomEventId(CancellationToken.None);

        // When
        var bookingInfo = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        var booking = await dbContext.GetBookingAsync(bookingInfo.Id, CancellationToken.None);

        // Then
        Assert.Equal(eventId, bookingInfo.EventId);
        Assert.Equal(eventId, booking?.EventId);

        Assert.Equal(BookingStatus.Pending, bookingInfo.Status);
        Assert.Equal(BookingStatus.Pending, booking?.Status);

        Assert.Null(bookingInfo.ProcessedAt);
        Assert.True(booking != null && booking?.ProcessedAt is null);
    }

    /// <summary>
    /// Создание несколько броней для одного события.
    /// </summary>
    /// <param name="bookingCount"></param>
    /// <returns></returns>
    [Trait(Category, Category_Booking)]
    [Theory]
    [InlineData([10])]
    [InlineData([20])]
    [InlineData([50])]
    public async Task CreateMultiBookingsForEvent_Success(int bookingCount)
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var eventId = await context.GetRandomEventId(CancellationToken.None);

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

    /// <summary>
    /// Получение информации о брони по идентификатору.
    /// </summary>
    /// <returns></returns>
    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task GetBookingById_Success()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        
        var eventId = await context.GetRandomEventId(CancellationToken.None);
        var booking = new Booking()
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now,
        };

        // When
        var bookingCreated = booking.ToInfo();
        await dbContext.AddBookingAsync(booking, CancellationToken.None);
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

    /// <summary>
    /// Тест асинхронной очереди.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task TestBookingQueue_Success()
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        var queue = serviceProvider.GetRequiredService<IAsyncQueue<Booking>>();
        var queuedBooking = new Booking()
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now,
        };

        // When
        var queueTask = queue.Dequeue(CancellationToken.None);
        await queue.Enqueue(queuedBooking, CancellationToken.None);
        var dequeuedBooking = await queueTask;

        // Then
        Assert.NotNull(dequeuedBooking);
        Assert.Equal(queuedBooking, dequeuedBooking);
    }

    /// <summary>
    /// Тест запуска/останова фоновой службы.
    /// </summary>
    /// <returns></returns>
    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task TestRunStopBackgroundService_Success()
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        var service = serviceProvider.GetRequiredService<IAppBackgroundService>();

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

    /// <summary>
    /// Тест изменения статуса брони при обработке фоновой службой.
    /// </summary>
    /// <returns></returns>
    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task TestChangeStatusOneEvent_Success()
    {
        // Given        
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var backService = serviceProvider.GetRequiredService<IAppBackgroundService>();
        
        var eventId = await context.GetRandomEventId(CancellationToken.None);

        // When
        var bookingCreated = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        var bookingBeforeChange = await bookingService.GetBookingByIdAsync(bookingCreated.Id, CancellationToken.None);

        await backService.StartAsync(CancellationToken.None);

        var bookingBeforeChange2 = await bookingService.GetBookingByIdAsync(bookingCreated.Id, CancellationToken.None);

        await Task.Delay(TimeSpan.FromSeconds(3));

        BookingInfo? bookingAfterChange;
        await using (var scope2 = serviceProvider.CreateAsyncScope())
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

    /// <summary>
    /// Тест неудачной попытки создать бронь для несуществующего события.
    /// </summary>
    /// <returns></returns>
    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task CreateBookingNotExistEvent_Fail()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventId = Guid.NewGuid();

        // Then
        await Assert.ThrowsAsync<EventNotFoundException>(() => bookingService.CreateBookingAsync(eventId, CancellationToken.None));
    }

    /// <summary>
    /// Тест неудачной попытки создать бронь для удаленного события.
    /// </summary>
    /// <returns></returns>
    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task CreateBookingForDeletedEvent_Fail()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        
        var eventId = await context.GetRandomEventId(CancellationToken.None);

        // When
        var deletingEvent = await dbContext.GetEventAsync(eventId, CancellationToken.None);
        dbContext.DeleteEvent(eventId);
        var deletedEvent = await dbContext.GetEventAsync(eventId, CancellationToken.None);

        // Then
        Assert.Equal(eventId, deletingEvent?.Id);
        Assert.Null(deletedEvent);
        await Assert.ThrowsAsync<EventNotFoundException>(() => bookingService.CreateBookingAsync(eventId, CancellationToken.None));
    }

    /// <summary>
    /// Тест неудачной попытки получить инфо о несуществующей брони.
    /// </summary>
    /// <returns></returns>
    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task GetBookingById_Fail()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        
        var bookingId = Guid.NewGuid();

        // Then
        await Assert.ThrowsAsync<BookingNotFoundException>(() => bookingService.GetBookingByIdAsync(bookingId, CancellationToken.None));
    }
}