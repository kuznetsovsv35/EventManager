using EventManager.Application.DataTransfer;

namespace EventManager.Tests;

public class ManageEventTest(EventServiceFixture fixture) : IClassFixture<EventServiceFixture>
{
    [Fact]
    public void CreateEvent_Success()
    {
        // Given
        var title = "Simple event";
        var startAt = new DateTime(2026, 6, 28, 10, 0, 00);
        var endAt = new DateTime(2026, 6, 28, 10, 30, 00);
        var description = "Some event";
        var expectedCount = fixture.DbContext.Events.Count() + 1;

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
        var actualCount = fixture.DbContext.Events.Count();
        Assert.Equal(expectedCount, actualCount);
        Assert.Equal(inData, outData);
    }

    [Fact]
    public void GetAllEvents_Success()
    {
        // Given
        var expected = fixture.DbContext.Events.AsEnumerable().Select(x => x.ToOutputData());
    
        // When
        var actual = fixture.Service.GetAllEvents().Values;
    
        // Then
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetEventByID_Success()
    {
        // Given
        var firstEvent = fixture.DbContext.Events.AsEnumerable().First();
        var requestedId = firstEvent.Id;
    
        // When
        var foundEvent = fixture.Service.GetEvent(requestedId);

        // Then
        Assert.NotNull(foundEvent);
        Assert.Equal(requestedId, foundEvent.Id);
    }

    [Fact]
    public void UpdateEvent_Success()
    {
        // Given
        EventInputData inputData = new()
        {
            Title = "Title updated",
            StartAt = new DateTime(1976, 1, 15, 15, 34, 0),
            EndAt = new DateTime(1976, 1, 15, 16, 34, 0),
            Description = "Description updated"
        };

        var requestedId = fixture.DbContext.Events.AsEnumerable().Last().Id;
    
        // When
        var outData = fixture.Service.UpdateEvent(requestedId, inputData);
    
        // Then
        Assert.NotNull(outData);
        Assert.Equal(requestedId, outData.Id);
        Assert.Equal(inputData, outData);
    }

    [Fact]
    public void DeleteEvent_Siccess()
    {
        // Given
        var requestedEvent = fixture.DbContext.Events.AsEnumerable().First();
        var requestedId = requestedEvent.Id;
        var expectedEvent = requestedEvent.ToOutputData();
        var expectedCount = fixture.DbContext.Events.Count() -1;
    
        // When
        var deletedEvent = fixture.Service.DeleteEvent(requestedId);
    
        // Then
        var actualCount = fixture.DbContext.Events.Count();
        var foundEvent = fixture.DbContext.Events.AsEnumerable().FirstOrDefault(e => e.Id == requestedId);
        Assert.Equal(expectedCount, actualCount);
        Assert.Equal(expectedEvent, deletedEvent);
        Assert.Null(foundEvent);
    }
}