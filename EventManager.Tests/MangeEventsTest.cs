using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;

namespace EventManager.Tests;

public class ManageEventTest(EventServiceFixture fixture) : IClassFixture<EventServiceFixture>
{
    [Fact]
    public void CreateEvent_Success()
    {
        var title = "Simple event";
        var startAt = new DateTime(2026, 6, 28, 10, 0, 00);
        var endAt = new DateTime(2026, 6, 28, 10, 30, 00);
        var description = "Some event";
        // Given
        EventInputData inData = new()
        {
            Title = title,
            StartAt = startAt,
            EndAt = endAt,
            Description = description
        };
    
        // When
        var outData = fixture.Service.CreateEvent(inData);
    
        // Then
        Assert.Equal(1, fixture.DbContext.Events.Count());

        Assert.Equal(title, outData.Title);
        Assert.Equal(startAt, outData.StartAt);
        Assert.Equal(endAt, outData.EndAt);
        Assert.Equal(description, outData.Description);
    }
}