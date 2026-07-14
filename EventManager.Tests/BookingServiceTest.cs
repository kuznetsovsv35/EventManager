using EventManager.Application.DataTransfer;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EventManager.Tests;

public class BookingServiceTest(BookingServiceFixture fixture) : TraitAttributes, IClassFixture<BookingServiceFixture>
{
    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task CreateBookingExistingEvent_Success()
    {
        // Given
        using var cts = new CancellationTokenSource();
        var eventId = (await fixture.Events.FirstAsync(cts.Token)).Id;
    
        // When
        var bookingInfo = await fixture.BookingService.CreateBookingAsync(eventId, cts.Token);
    
        // Then
        Assert.Equal(eventId, bookingInfo.EventId);
        Assert.Equal(BookingStatus.Pending, bookingInfo.Status);
        Assert.Null(bookingInfo.ProcessedAt);
    }

    [Trait(Category, Category_Booking)]
    [Theory]
    [InlineData([10])]
    [InlineData([20])]
    [InlineData([50])]
    public async Task CreateMultiBookingsForEvent_Success(int bookingCount)
    {
        // Given
        using var cts = new CancellationTokenSource();
        var eventCount = await fixture.Events.CountAsync(cts.Token);
        var eventIndex = Random.Shared.Next(eventCount);
        var eventId = (await fixture.Events.Skip(eventIndex).FirstAsync(cts.Token)).Id;
    
        // When
        Task<BookingInfo>[] tasks = [.. Enumerable
            .Range(0, bookingCount)
            .Select(_ => fixture.BookingService.CreateBookingAsync(eventId, cts.Token))];
        
        var bookingIds = tasks.Select(t => t.Result.Id).ToHashSet();
    
        // Then
        Assert.Equal(bookingCount, bookingIds.Count);
    }

    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task GetBookingById_Success()
    {
        // Given
        using var cts = new CancellationTokenSource();
        var eventCount = await fixture.Events.CountAsync(cts.Token);
        var eventIndex = Random.Shared.Next(eventCount);
        var eventId = (await fixture.Events.Skip(eventIndex).FirstAsync(cts.Token)).Id;

        // When
        var bookingCreated = await fixture.BookingService.CreateBookingAsync(eventId, cts.Token);
        var bookingFound = await fixture.BookingService.GetBookingByIdAsync(bookingCreated.Id, cts.Token);
    
        // Then
        Assert.Equal(bookingCreated.Id, bookingFound.Id);
        Assert.Equal(eventId, bookingCreated.EventId);
        Assert.Equal(bookingCreated.EventId, bookingFound.EventId);
        Assert.Equal(BookingStatus.Pending, bookingCreated.Status);
        Assert.Equal(bookingCreated.Status, bookingFound.Status);
        Assert.Null(bookingCreated.ProcessedAt);
        Assert.Null(bookingFound.ProcessedAt);
    }

    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task TestRunStopBackgroudService_Success()
    {
        // Given
        var cts = new CancellationTokenSource();
        await fixture.BookingQueue.Clear();
     
        // When
        await fixture.BackgroudService.StartAsync(cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(5));
        await fixture.BackgroudService.StopAsync(cts.Token);
    
        // Then
    }
}