using EventManager.Application.DataTransfer;

namespace EventManager.Tests;

public class FilterEventTest(EventServiceFixture fixture) : IClassFixture<EventServiceFixture>
{
    [Fact]
    public void SimpleFilterByTitle_Success()
    {
        // Given
        const string titleAll = "Event Title";  // all event expected
        const string titleNone = "AbcDeF";      // No events        
        var expectedAll = fixture.DbContext.Events.AsEnumerable().Select(x => x.ToOutputData());
    
        // When
        var actualAll = fixture.Service.GetEvents(new FilterParams(titleAll, null, null), PageParams.NoPages).Values;
        var actualdNone = fixture.Service.GetEvents(new FilterParams(titleNone, null, null), PageParams.NoPages).Values;
    
        // Then
        Assert.Equal(expectedAll, actualAll);
        Assert.Empty(actualdNone);
    }

    public static readonly IEnumerable<object[]> Titles = Enumerable.Range(1, TestAppDbContext.EventCount)
        .Select(i => new object[]{$"Event Title {i}"})
        .ToArray();

    [Theory]
    [MemberData(nameof(Titles))]
    public void IterationFilterByTitle_Success(string title)
    {
        var expected = fixture.DbContext.Events.Where(x => x.Title.Contains(title)).AsEnumerable().Select(x => x.ToOutputData());
        var actual = fixture.Service.GetEvents(new FilterParams(title, null, null), PageParams.NoPages).Values;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FilterByDate_Success()
    {
        // Given
    
        // When
    
        // Then
    }
}