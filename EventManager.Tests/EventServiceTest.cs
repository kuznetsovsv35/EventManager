using EventManager.Application.DataTransfer;

namespace EventManager.Tests;

public class EventServiceTest(EventServiceFixture fixture) : IClassFixture<EventServiceFixture>
{
    ////////////////////////////////////////////////////////////////////////////////////////////
    /// Тесты управления событиями.
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Fact]
    public void CreateEvent_Success()
    {
        // Given
        var title = "Simple event";
        var startAt = new DateTime(2026, 6, 28, 10, 0, 00);
        var endAt = new DateTime(2026, 6, 28, 10, 30, 00);
        var description = "Some event";
        var expectedCount = fixture.Events.Count() + 1;

        EventInputData inData = new()
        {
            Title = title,
            StartAt = startAt,
            EndAt = endAt,
            Description = description
        };
    
        // When
        var outData = fixture.AppService.CreateEvent(inData);
    
        // Then
        var actualCount = fixture.Events.Count();
        Assert.Equal(expectedCount, actualCount);
        Assert.Equal(inData, outData);
    }

    [Fact]
    public void GetAllEvents_Success()
    {
        // Given
        var expected = fixture.Events.Select(x => x.ToOutputData());
    
        // When
        var actual = fixture.AppService.GetAllEvents();
    
        // Then
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetEventByID_Success()
    {
        // Given
        var firstEvent = fixture.Events.First();
        var requestedId = firstEvent.Id;
    
        // When
        var foundEvent = fixture.AppService.GetEvent(requestedId);

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

        var requestedId = fixture.Events.Last().Id;
    
        // When
        var outData = fixture.AppService.UpdateEvent(requestedId, inputData);
    
        // Then
        Assert.NotNull(outData);
        Assert.Equal(requestedId, outData.Id);
        Assert.Equal(inputData, outData);
    }

    [Fact]
    public void DeleteEvent_Success()
    {
        // Given
        var requestedEvent = fixture.Events.First();
        var requestedId = requestedEvent.Id;
        var expectedEvent = requestedEvent.ToOutputData();
        var expectedCount = fixture.Events.Count() -1;
    
        // When
        var deletedEvent = fixture.AppService.DeleteEvent(requestedId);
    
        // Then
        var actualCount = fixture.Events.Count();
        var foundEvent = fixture.Events.FirstOrDefault(e => e.Id == requestedId);
        Assert.Equal(expectedCount, actualCount);
        Assert.Equal(expectedEvent, deletedEvent);
        Assert.Null(foundEvent);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    /// Тесты фильтров в комплексе.
    ////////////////////////////////////////////////////////////////////////////////////////////
    [Fact]
    public void SimpleFilterByTitle_Success()
    {
        // Given
        const string titleAll = "Event Title";  // all event expected
        const string titleNone = "AbcDeF";      // No events        
        var expectedAll = fixture.Events
            .Where(x => x.Title.Contains(titleAll))
            .Select(x => x.ToOutputData());
        var expectedCount = expectedAll.Count();
    
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
    [InlineData(["Event Title 1"])]
    [InlineData(["Event Title 2"])]
    [InlineData(["Event Title 3"])]
    public void PartialFilterByTitle_Success(string title)
    {
        var expected = fixture.Events.Where(x => x.Title.Contains(title)).Select(x => x.ToOutputData());
        
        var actual = fixture.AppService.GetEvents(
            new() { Title = title}, 
            PageParams.NoPages)
            .Values;

        Assert.All(actual, item => Assert.Contains(title, item.Title));
        Assert.Equal(expected, actual);
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
            [new DateTime(2026, 1, 1)],
            [new DateTime(2026, 6, 28)],
            [new DateTime(2026, 6, 30)],
            [new DateTime(2026, 7, 10)],
            [new DateTime(2026, 7, 20)],
            [new DateTime(2026, 7, 28)],
            [new DateTime(2026, 8, 10)],
        ];
    [Theory]
    [MemberData(nameof(StartDates))]
    public void FilterByStartDate_Success(DateTime startAt)
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
    }

    public static readonly IEnumerable<object[]> EndDates =
        [
            [new DateTime(2026, 1, 1)],
            [new DateTime(2026, 6, 28)],
            [new DateTime(2026, 6, 30)],
            [new DateTime(2026, 7, 10)],
            [new DateTime(2026, 7, 20)],
            [new DateTime(2026, 7, 28)],
            [new DateTime(2026, 8, 10)],
        ];
    [Theory]
    [MemberData(nameof(EndDates))]
    public void FilterByEndDate_Success(DateTime endAt)
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
    }

    public static readonly IEnumerable<object[]> Combined =
        [
            ["Event", new DateTime(2026, 5, 1), new DateTime(2026, 5, 2)],
            ["Title", new DateTime(2026, 6, 28), new DateTime(2026, 6, 30)],
            [null!, new DateTime(2026, 6, 30), null!],
            ["bcd", new DateTime(2026, 7, 10), new DateTime(2026, 7, 15)],
            ["Ev", null!, new DateTime(2026, 7, 21)],
            ["Ti", new DateTime(2026, 7, 27), new DateTime(2026, 7, 20)],
            ["nt Ti", new DateTime(2026, 8, 10), new DateTime(2026, 8, 10)],
        ];

    [Theory]
    [MemberData(nameof(Combined))]
    public void CombinedFilter_Success(string? title, DateTime? startAt, DateTime? endAt)
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
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    /// Тесты разбивки на страницы в комплексе.
    ////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Тест разбивки на страницы.
    /// </summary>
    /// <param name="page">Запрашиваемая страница.</param>
    /// <param name="pageSize">Требуемый размер страницы.</param>
    /// <param name="expectedPageCount">Ожидаемое количество страниц.</param>
    /// <param name="expectedPageSize">Ожидаемое выведенных элементов на запрашиваемой страницы.</param>
    [Theory]
    [InlineData([1, 10, 3, 10])]
    [InlineData([3, 10, 3, 10])]
    [InlineData([4, 10, 3, 0])]
    [InlineData([2, 7, 5, 7])]
    [InlineData([5, 7, 5, 2])]
    [InlineData([1, 30, 1, 30])]
    public void PaginateResult_Success(int page, int pageSize, int expectedPageCount, int expectedPageSize)
    {
        // Given
        var allValues = fixture.AppService.GetAllEvents().ToList();
        var expectedTotalCount = allValues.Count;
        var expectedValues = allValues
            .Skip((page -1) * pageSize)
            .Take(pageSize);

        // When
        var pageResult = fixture.AppService.GetEvents(null, new(){ CurrentPage = page, PageSize = pageSize });
        
        // Then
        Assert.Equal(expectedPageCount, pageResult.PageCount);
        Assert.Equal(page, pageResult.PageNumber);
        Assert.Equal(expectedPageSize, pageResult.PageSize);
        Assert.Equal(expectedTotalCount, pageResult.TotalCount);
        Assert.Equal(expectedPageSize, pageResult.Values.Count());
        Assert.Equal(expectedValues, pageResult.Values);
    }
}