using EventManager.Application.DataTransfer;
using EventManager.Models;

namespace EventManager.Tests;

public class BookingServiceTest : TraitAttributes
{
    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task CreateBookingExistingEvent_Success()
    {
        // Given
        var ctx = new BookingServiceTestContext();
        using var cts = new CancellationTokenSource();
        var eventId = await ctx.GetRandomEventId(cts.Token);
    
        // When
        var bookingInfo = await ctx.BookingService.CreateBookingAsync(eventId, cts.Token);
    
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
        var ctx = new BookingServiceTestContext();
        using var cts = new CancellationTokenSource();
        var eventId = await ctx.GetRandomEventId(cts.Token);
        var bookingService = ctx.BookingService;
    
        // When
        var bookingIds = Enumerable
            .Range(0, bookingCount)
            .Select(async _ => await bookingService.CreateBookingAsync(eventId, cts.Token))
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
        using var cts = new CancellationTokenSource();
        var eventId = await ctx.GetRandomEventId(cts.Token);
        var booking = new Booking()
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now,
        };

        // When
        var bookingCreated = booking.ToInfo();
        await ctx.DbContext.AddBookingAsync(booking, cts.Token);
        var bookingFound = await ctx.BookingService.GetBookingByIdAsync(booking.Id, cts.Token);
    
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
    public async Task TestRunStopBackgroundService_Success()
    {
        // Given
        var ctx = new BookingServiceTestContext();
        using var cts = new CancellationTokenSource();
     
        // When
        await ctx.BackgroundService.StartAsync(cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(2));
        await ctx.BackgroundService.StopAsync(cts.Token);
    
        // Then
    }

    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task TestChangeStatusOneEvent()
    {
        // Given        
        var ctx = new BookingServiceTestContext();
        using var cts = new CancellationTokenSource();
        var eventId = await ctx.GetRandomEventId(cts.Token);
    
        // When
        var bookingCreated = await ctx.BookingService.CreateBookingAsync(eventId, cts.Token);
        var bookingBeforeChange = await ctx.BookingService.GetBookingByIdAsync(bookingCreated.Id, cts.Token);
        
        await ctx.BackgroundService.StartAsync(cts.Token);
        
        var bookingBeforeChange2 = await ctx.BookingService.GetBookingByIdAsync(bookingCreated.Id, cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);
        var bookingAfterChange = await ctx.BookingService.GetBookingByIdAsync(bookingCreated.Id, cts.Token);
        
        await ctx.BackgroundService.StopAsync(cts.Token);
    
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