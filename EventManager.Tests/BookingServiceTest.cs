using EventManager.Application.DataTransfer;
using EventManager.Models;

namespace EventManager.Tests;

public class BookingServiceTest(BookingServiceFixture fixture) : TraitAttributes, IClassFixture<BookingServiceFixture>
{
    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task CreateBookingExistingEvent_Success()
    {
        // Given
        using var cts = new CancellationTokenSource();
        var eventId = await fixture.GetRandomEventId(cts.Token);
    
        // When
        var bookingInfo = await fixture.BookingService.CreateBookingAsync(eventId, cts.Token);
        await fixture.BookingQueue.Clear();
    
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
        var eventId = await fixture.GetRandomEventId(cts.Token);
    
        // When
        var bookingIds = Enumerable
            .Range(0, bookingCount)
            .Select(async _ => await fixture.BookingService.CreateBookingAsync(eventId, cts.Token))
            .ToHashSet();
        
        await fixture.BookingQueue.Clear();
    
        // Then
        Assert.Equal(bookingCount, bookingIds.Count);
    }

    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task GetBookingById_Success()
    {
        // Given
        using var cts = new CancellationTokenSource();
        var eventId = await fixture.GetRandomEventId(cts.Token);
        var booking = new Booking()
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now,
        };

        // When
        var bookingCreated = booking.ToInfo();
        await fixture.AddBookingAsync(booking, cts.Token);
        var bookingFound = await fixture.BookingService.GetBookingByIdAsync(booking.Id, cts.Token);
    
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

    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task TestChangeStatusOneEvent()
    {
        // Given        
        var cts = new CancellationTokenSource();
        var eventId = await fixture.GetRandomEventId(cts.Token);
        await fixture.BookingQueue.Clear();
    
        // When
        var bookingCreated = await fixture.BookingService.CreateBookingAsync(eventId, cts.Token);
        var bookingBeforeChange = await fixture.BookingService.GetBookingByIdAsync(bookingCreated.Id, cts.Token);
        
        await fixture.BackgroudService.StartAsync(cts.Token);
        
        var bookingBeforeChange2 = await fixture.BookingService.GetBookingByIdAsync(bookingCreated.Id, cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
        var bookingAfterChange = await fixture.BookingService.GetBookingByIdAsync(bookingCreated.Id, cts.Token);
        
        await fixture.BackgroudService.StopAsync(cts.Token);
    
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
}