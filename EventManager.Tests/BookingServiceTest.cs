using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Data;
using EventManager.Infrastructure;
using EventManager.Models;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Tests;

/// <summary>
/// Тесты сервиса бронирования.
/// </summary>
public class BookingServiceTest(EventManagerTestContext context) : TestObjectBase, IClassFixture<EventManagerTestContext>
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
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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
    /// <returns></returns>
    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task CreateMultiBookingsForEvent_Success()
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        var eventId = await context.GetRandomEventId(CancellationToken.None);

        async Task<int> GetAvailableSeats()
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            return (await eventService.GetEventAsync(eventId, CancellationToken.None)).AvailableSeats;
        }

        var bookingCount = await GetAvailableSeats();

        // When
        var bookingTasks = Enumerable
            .Range(0, bookingCount)
            .Select(async (_) =>
            {
                await using var scope = serviceProvider.CreateAsyncScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                return await bookingService.CreateBookingAsync(eventId, CancellationToken.None);
            })
            .ToList();

        var bookingIds = (await Task.WhenAll(bookingTasks)).Select(t => t.Id).ToList();
        var availableSeats = await GetAvailableSeats();

        // Then
        Assert.True(bookingCount > 0);
        Assert.Equal(bookingCount, bookingIds.Count);
        Assert.Distinct(bookingIds);
        Assert.Equal(0, availableSeats);
    }

    /// <summary>
    /// Тест лимита бронирования (попыток вдвое больше чем мест для случайного события).
    /// </summary>
    /// <returns></returns>
    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task TestSeatLimit_Success()
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        var eventId = await context.GetRandomEventId(CancellationToken.None);

        async Task<int> GetAvailableSeats()
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            return (await eventService.GetEventAsync(eventId, CancellationToken.None)).AvailableSeats;
        }

        var bookingCount = await GetAvailableSeats() * 2;
        var expectedSuccessCount = bookingCount / 2;
        var expectedFailCount = bookingCount - expectedSuccessCount;

        var successCount = 0;
        var failCount = 0;
        var processedCount = 0;
        var availableSeatsCount = 0;

        // When
        await Task.WhenAll(Enumerable
            .Range(0, bookingCount)
            .Select(async (_) =>
            {
                try
                {
                    await using var scope = serviceProvider.CreateAsyncScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    await bookingService.CreateBookingAsync(eventId, CancellationToken.None);
                    Interlocked.Increment(ref successCount);
                }
                catch (NoAvailableSeatsException)
                {
                    Interlocked.Increment(ref failCount);
                    Interlocked.Add(ref availableSeatsCount, await GetAvailableSeats());
                }
                Interlocked.Increment(ref processedCount);
            }));

        // Then
        Assert.Equal(bookingCount, processedCount);
        Assert.Equal(bookingCount, expectedSuccessCount + expectedFailCount);
        Assert.Equal(bookingCount, successCount + failCount);
        Assert.Equal(expectedSuccessCount, successCount);
        Assert.Equal(expectedFailCount, failCount);
        Assert.Equal(0, availableSeatsCount);
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
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var eventId = await context.GetRandomEventId(CancellationToken.None);
        var booking = new Booking(eventId);

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
        var queue = serviceProvider.GetRequiredService<IAsyncQueue<Guid>>();
        var queuedBooking = Guid.NewGuid();

        // When
        var queueTask = queue.Dequeue(CancellationToken.None);
        await queue.Enqueue(queuedBooking, CancellationToken.None);
        var dequeuedBooking = await queueTask;

        // Then
        Assert.NotEqual(dequeuedBooking, Guid.Empty);
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
    /// Тест на исключение в случае отсутствия свободных мест.
    /// </summary>
    /// <returns></returns>
    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task CreateBookingNoAvailableSeats_Fail()
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();

        EventInputData inputData = new()
        {
            Title = "Simple event",
            StartAt = new DateTime(2026, 6, 28, 10, 0, 00),
            EndAt = new DateTime(2026, 6, 28, 10, 30, 00),
            Description = "Some event",
            TotalSeats = 1,
        };

        async Task<Guid> CreateEvent()
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            return (await eventService.CreateEventAsync(inputData, CancellationToken.None)).Id;
        }

        async Task CreateBooking(Guid eventId)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        }

        // When
        var eventId = await CreateEvent();
        await CreateBooking(eventId);

        // Then
        await Assert.ThrowsAsync<NoAvailableSeatsException>(async () => await CreateBooking(eventId));
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
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var eventId = await context.GetRandomEventId(CancellationToken.None);

        // When
        var deletingEvent = await dbContext.GetEventAsync(eventId, CancellationToken.None);
        await dbContext.DeleteEventAsync(eventId, CancellationToken.None);
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

    /// <summary>
    /// Тест на поведение в случае отклонения брони.
    /// </summary>
    /// <returns></returns>
    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task TestRejectedBooking_Success()
    {
        // Given        
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var backService = serviceProvider.GetRequiredService<IAppBackgroundService>();
        backService.ProcessBooking += (sender, booking) => booking.Reject();

        var eventId = await context.GetRandomEventId(CancellationToken.None);
        var originAvailableSeats = (await eventService.GetEventAsync(eventId, CancellationToken.None)).AvailableSeats;

        // When
        async Task<int> GetAvailableSeats(Guid guid)
        {
            await using (var scope2 = serviceProvider.CreateAsyncScope())
            {
                return (await scope2.ServiceProvider
                    .GetRequiredService<IEventService>()
                    .GetEventAsync(guid, CancellationToken.None)).AvailableSeats;
            }
        }

        async Task<BookingStatus> GetBookingStatus(Guid guid)
        {
            await using (var scope2 = serviceProvider.CreateAsyncScope())
            {
                return (await scope2.ServiceProvider
                    .GetRequiredService<IBookingService>()
                    .GetBookingByIdAsync(guid, CancellationToken.None)).Status;
            }
        }

        var bookingId = (await bookingService.CreateBookingAsync(eventId, CancellationToken.None)).Id;

        var availableSeatsAfterBooking = await GetAvailableSeats(eventId);

        await backService.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(3));
        await backService.StopAsync(CancellationToken.None);

        int availableSeatsAfterReject = await GetAvailableSeats(eventId);
        var bookingStatus = await GetBookingStatus(bookingId);

        // Then
        Assert.Equal(BookingStatus.Rejected, bookingStatus);
        Assert.Equal(originAvailableSeats, availableSeatsAfterBooking + 1);
        Assert.Equal(originAvailableSeats, availableSeatsAfterReject);
    }
}