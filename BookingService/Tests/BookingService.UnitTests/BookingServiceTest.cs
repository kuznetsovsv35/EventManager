using BookingService.Application;
using BookingService.Application.DTO;
using BookingService.Domain;
using BookingService.Domain.Exceptions;
using BookingService.Infrastructure;
using Common.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.UnitTests;

/// <summary>
/// Тесты сервиса бронирования.
/// </summary>
public class BookingServiceTest(TestContext context) : IClassFixture<TestContext>
{
    /// <summary>
    /// Создание брони для существующего события.
    /// </summary>
    /// <returns></returns>
    [Trait(Category.Name, Category.Booking)]
    [Fact]
    public async Task CreateBookingExistingEvent_Success()
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        var eventId = Guid.NewGuid();

        // When
        var bookingInfo = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        var booking = await dbContext
            .Bookings
            .AsNoTracking()
            .SingleAsync(b => b.Id == bookingInfo.Id, CancellationToken.None);

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
    [Trait(Category.Name, Category.Booking)]
    [Fact]
    public async Task CreateMultiBookingsForEvent_Success()
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        var eventId = Guid.NewGuid();

        var bookingCount = 10;

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

        // Then
        Assert.True(bookingCount > 0);
        Assert.Equal(bookingCount, bookingIds.Count);
        Assert.Distinct(bookingIds);
    }

    /// <summary>
    /// Получение информации о брони по идентификатору.
    /// </summary>
    /// <returns></returns>
    [Trait(Category.Name, Category.Booking)]
    [Fact]
    public async Task GetBookingById_Success()
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var eventId = Guid.NewGuid();
        var booking = new Booking(eventId, Guid.Empty);

        // When
        var bookingCreated = booking.ToInfo();
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();
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
    /// Тест запуска/останова фоновой службы.
    /// </summary>
    /// <returns></returns>
    [Trait(Category.Name, Category.Booking)]
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
    [Trait(Category.Name, Category.Booking)]
    [Fact]
    public async Task TestChangeStatusOneEvent_Success()
    {
        // Given        
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var backService = serviceProvider.GetRequiredService<IAppBackgroundService>();

        var eventId = Guid.NewGuid();

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
    /// Тест неудачной попытки получить инфо о несуществующей брони.
    /// </summary>
    /// <returns></returns>
    [Trait(Category.Name, Category.Booking)]
    [Fact]
    public async Task GetBookingById_Fail()
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var bookingId = Guid.NewGuid();

        // Then
        await Assert.ThrowsAsync<BookingNotFoundException>(() => bookingService.GetBookingByIdAsync(bookingId, CancellationToken.None));
    }

    /// <summary>
    /// Тест на поведение в случае отклонения брони.
    /// </summary>
    /// <returns></returns>
    [Trait(Category.Name, Category.Booking)]
    [Fact]
    public async Task TestRejectedBooking_Success()
    {
        // Given        
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var backService = serviceProvider.GetRequiredService<IAppBackgroundService>();
        backService.ProcessBooking += (sender, booking) => booking.Reject();

        var eventId = Guid.NewGuid();

        // When
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

        await backService.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(3));
        await backService.StopAsync(CancellationToken.None);

        var bookingStatus = await GetBookingStatus(bookingId);

        // Then
        Assert.Equal(BookingStatus.Rejected, bookingStatus);
    }
}