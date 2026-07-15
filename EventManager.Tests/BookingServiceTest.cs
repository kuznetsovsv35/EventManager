using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Models;
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
        var eventId = await ctx.GetRandomEventId(CancellationToken.None);
        var bookingService = ctx.GetService<IBookingService>();
    
        // When
        var bookingInfo = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);
    
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
        var eventId = await ctx.GetRandomEventId(CancellationToken.None);
        var bookingService = ctx.GetService<IBookingService>();
    
        // When
        var bookingIds = Enumerable
            .Range(0, bookingCount)
            .Select(async _ => await bookingService.CreateBookingAsync(eventId, CancellationToken.None))
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
        var bookingService = ctx.GetService<IBookingService>();
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
        await ctx.DbContext.AddBookingAsync(booking, CancellationToken.None);
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

    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task TestRunStopBackgroundService_Success()
    {
        // Given
        var ctx = new BookingServiceTestContext();
     
        // When
        var statusBeforeStart = ctx.BackgroundService.Status;
        await ctx.BackgroundService.StartAsync(CancellationToken.None);
        var statusAfterStart = ctx.BackgroundService.Status;
        
        await Task.Delay(TimeSpan.FromSeconds(2));
        var statusRunning = ctx.BackgroundService.Status;

        await ctx.BackgroundService.StopAsync(CancellationToken.None);
        var statusAfterStop = ctx.BackgroundService.Status;

        await Task.Delay(TimeSpan.FromSeconds(2));
        var statusStopped = ctx.BackgroundService.Status;

        // Then
        Assert.Equal(BackgroundServiceStatus.Stopped, statusBeforeStart);
        Assert.NotEqual(BackgroundServiceStatus.Stopped, statusAfterStart);
        Assert.Equal(BackgroundServiceStatus.Runing, statusRunning);
        Assert.NotEqual(BackgroundServiceStatus.Runing, statusAfterStop);
        Assert.Equal(BackgroundServiceStatus.Stopped, statusStopped);
    }

    [Trait(Category, Category_Booking)]
    [Fact]
    public async Task TestChangeStatusOneEvent_Success()
    {
        // Given        
        var ctx = new BookingServiceTestContext();
        var bookingService = ctx.ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<IBookingService>();
        var eventId = await ctx.GetRandomEventId(CancellationToken.None);
    
        // When
        var bookingCreated = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        var bookingBeforeChange = await bookingService.GetBookingByIdAsync(bookingCreated.Id, CancellationToken.None);
        
        await ctx.BackgroundService.StartAsync(CancellationToken.None);
        
        var bookingBeforeChange2 = await bookingService.GetBookingByIdAsync(bookingCreated.Id, CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(3));
        
        bookingService = ctx.ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<IBookingService>();
        var bookingAfterChange = await bookingService.GetBookingByIdAsync(bookingCreated.Id, CancellationToken.None);
        
        await ctx.BackgroundService.StopAsync(CancellationToken.None);
    
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