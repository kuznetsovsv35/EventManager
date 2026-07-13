using Microsoft.EntityFrameworkCore;

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
    }
}