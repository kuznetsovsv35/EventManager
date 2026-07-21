using EventManager.Application.DataTransfer;
using EventManager.Models;

namespace EventManager.Tests;

public class ValidationTest : TraitAttributes
{
    [Trait(Category, Category_Validation)]
    [Fact]
    public async Task ReserveSeats_Success()
    {
        // Given
        const int initialTotalSeats = 5;
        const int eventDuration = 2; // 2 hours
        const int acceptedCount = 2;
        const int failCount = 7;
    
        // When
        var startAt = DateTime.Now;
        var endStart = startAt.AddHours(eventDuration);

        EventInputData eventInputData = new()
        {            
            Title = "Event Title",
            StartAt = startAt,
            EndAt = endStart,
            TotalSeats = initialTotalSeats,
        };

        Event @event = eventInputData.ToEvent();
        var afterCreate = @event.ToOutputData();
        
        var acceptedReserve = @event.TryReserveSeats(acceptedCount);
        var afterSuccessReserve = @event.ToOutputData();
    
        var failReserve = @event.TryReserveSeats(failCount);
        var afterFailReserve = @event.ToOutputData();

        // Then
        Assert.Equal(initialTotalSeats, afterCreate.TotalSeats);
        Assert.Equal(initialTotalSeats, afterCreate.AvailableSeats);

        Assert.True(acceptedReserve);
        Assert.Equal(initialTotalSeats, afterSuccessReserve.TotalSeats);
        Assert.Equal(initialTotalSeats - acceptedCount, afterSuccessReserve.AvailableSeats);

        Assert.False(failReserve);
        Assert.Equal(initialTotalSeats, afterFailReserve.TotalSeats);
        Assert.Equal(afterSuccessReserve.AvailableSeats, afterFailReserve.AvailableSeats);
    }

}