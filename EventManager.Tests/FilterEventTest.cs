using EventManager.Application.DataTransfer;

namespace EventManager.Tests;

public class FilterEventTest(FilterFixture fixture) : IClassFixture<PaginatorFixture>
{
    [Fact]
    public void SimpleFilterByTitle_Success()
    {
        // Given
        const string titleAll = "Event Title";  // all event expected
        const string titleNone = "AbcDeF";      // No events        
        var expectedAll = fixture.Events.Select(x => x.ToOutputData());
        var expectedCount = fixture.Events.Count();
    
        // When
        var actualAll = fixture.AppService.GetEvents(
            new() { Title = titleAll }, 
            PageParams.NoPages)
            .Values;
        
        var actualNone = fixture.AppService.GetEvents(
            new () { Title = titleNone }, 
            PageParams.NoPages)
            .Values;
    
        // Then
        Assert.Equal(expectedAll, actualAll);
        Assert.All(actualAll, item => Assert.Contains(titleAll, item.Title));
        Assert.Equal(expectedCount, actualAll.Count());
        Assert.Empty(actualNone);
    }

    [Theory]
    [InlineData(["Event Title 1", 11])]
    [InlineData(["Event Title 2", 11])]
    [InlineData(["Event Title 3", 2])]
    public void PartialFilterByTitle_Success(string title, int expectedCount)
    {
        var expected = fixture.Events.Where(x => x.Title.Contains(title)).Select(x => x.ToOutputData());
        
        var actual = fixture.AppService.GetEvents(
            new() { Title = title}, 
            PageParams.NoPages)
            .Values;

        Assert.All(actual, item => Assert.Contains(title, item.Title));
        Assert.Equal(expected, actual);
        Assert.Equal(expectedCount, actual.Count());
    }

    public static readonly IEnumerable<object[]> Titles 
        = [.. Enumerable.Range(1, TestAppDbContext.EventCount).Select(i => new object[]{$"Event Title {i}"})];

    [Theory]
    [MemberData(nameof(Titles))]
    public void IterationFilterByTitle_Success(string title)
    {
        var expected = fixture.Events.Where(x => x.Title.Contains(title)).Select(x => x.ToOutputData());
        
        var actual = fixture.AppService.GetEvents(
            new() { Title =title }, 
            PageParams.NoPages)
            .Values;

        Assert.All(actual, item => Assert.Contains(title, item.Title));
        Assert.Equal(expected, actual);
        Assert.True(actual.Any());
    }

    public static readonly IEnumerable<object[]> StartDates =
        [
            [new DateTime(2026, 1, 1), 30],
            [new DateTime(2026, 6, 28), 30],
            [new DateTime(2026, 6, 30), 28],
            [new DateTime(2026, 7, 10), 18],
            [new DateTime(2026, 7, 20), 8],
            [new DateTime(2026, 7, 28), 0],
            [new DateTime(2026, 8, 10), 0],
        ];
    [Theory]
    [MemberData(nameof(StartDates))]
    public void FilterByStartDate_Success(DateTime startAt, int expectedCount)
    {
        // Given
        var expected = fixture.Events.Where(x => x.StartAt >= startAt).Select(x => x.ToOutputData());

        // When
        var actual = fixture.AppService.GetEvents(
            new() { From = startAt }
            , PageParams.NoPages).Values;
    
        // Then
        Assert.Equal(expected, actual);
        Assert.All(actual, item => Assert.True(item.StartAt >= startAt));
        Assert.Equal(expectedCount, expected.Count());
    }

    public static readonly IEnumerable<object[]> EndDates =
        [
            [new DateTime(2026, 1, 1), 0],
            [new DateTime(2026, 6, 28), 1],
            [new DateTime(2026, 6, 30), 3],
            [new DateTime(2026, 7, 10), 13],
            [new DateTime(2026, 7, 20), 23],
            [new DateTime(2026, 7, 28), 30],
            [new DateTime(2026, 8, 10), 30],
        ];
    [Theory]
    [MemberData(nameof(EndDates))]
    public void FilterByEndDate_Success(DateTime endAt, int expectedCount)
    {
        // Given
        var endNextDay = endAt.AddDays(1).Date;
        var expected = fixture.Events.Where(x => x.EndAt < endNextDay).Select(x => x.ToOutputData());

        // When
        var actual = fixture.AppService.GetEvents(
            new(){ To = endAt }
            , PageParams.NoPages).Values;
    
        // Then
        Assert.Equal(expected, actual);
        Assert.All(actual, item => Assert.True(item.EndAt < endNextDay));
        Assert.Equal(expectedCount, actual.Count());
    }

    public static readonly IEnumerable<object[]> Combined =
        [
            ["Event", new DateTime(2026, 5, 1), new DateTime(2026, 5, 2), 0],
            ["Title", new DateTime(2026, 6, 28), new DateTime(2026, 6, 30), 3],
            [null!, new DateTime(2026, 6, 30), null!, 28],
            ["bcd", new DateTime(2026, 7, 10), new DateTime(2026, 7, 15), 0],
            ["Ev", null!, new DateTime(2026, 7, 21), 24],
            ["Ti", new DateTime(2026, 7, 27), new DateTime(2026, 7, 20), 0],
            ["nt Ti", new DateTime(2026, 8, 10), new DateTime(2026, 8, 10), 0],
        ];

    [Theory]
    [MemberData(nameof(Combined))]
    public void CombinedFilter_Success(string? title, DateTime? startAt, DateTime? endAt, int expectedCount)
    {
        // Given
        var endNextDay = endAt?.AddDays(1).Date;
        
        var expected = fixture.Events
            .Where(x => (string.IsNullOrEmpty(title) || x.Title.Contains(title))
                && (startAt == null || x.StartAt >= startAt.Value)
                && (endNextDay == null || x.EndAt < endNextDay.Value))
            .Select(x => x.ToOutputData());

        // When
        var actual = fixture.AppService.GetEvents(
            new() { Title = title, From = startAt, To = endAt}
            , PageParams.NoPages).Values;
    
        // Then
        Assert.Equal(expected, actual);
        
        Assert.All(actual, item => 
            {
                if (title != null)
                    Assert.Contains(title, item.Title);
                
                if (startAt.HasValue)
                    Assert.True(item.StartAt >= startAt);

                if (endNextDay.HasValue)
                    Assert.True(item.EndAt < endNextDay);
            });

        Assert.Equal(expectedCount, actual.Count());
    }
}
