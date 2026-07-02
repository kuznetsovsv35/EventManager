using System.Linq.Expressions;
using EventManager.Models;

namespace EventManager.Tests;

/// <summary>
/// Модульные тесты сервиса фильтрации.
/// </summary>
/// <param name="fixture"></param>
public class FilterEventTest(FilterEventFixture fixture) : TraitAttributes, IClassFixture<FilterEventFixture>
{
    [Trait(Category, Category_Filters)]
    [Fact]
    public void Reset_Success()
    {
        // Given

        // When
        var filter = fixture.FilterService.Reset();

        // Then
        Assert.Null(filter.Expression);
    }

    [Trait(Category, Category_Filters)]
    [Fact]
    public void AddNullCondition_Fail()
    {
        // Given
        var filter = fixture.FilterService.Reset();

        // Then
        var ex = Assert.Throws<ArgumentNullException>(() => filter.AddCondition(null!));
        Assert.NotNull(ex.ParamName);
        Assert.NotEmpty(ex.ParamName);
    }

    [Trait(Category, Category_Filters)]
    [Fact]
    public void SimpleFilterByTitle_Success()
    {
        // Given
        const string titleAll = "Event title";
        const string titleNone = "AbcDeF";
        var titleAllLowCase = titleAll.ToLower();
        var titleNoneLowCase = titleNone.ToLower();

        Expression<Func<Event, bool>> exprTitleAll = x => x.Title.ToLower().Contains(titleAllLowCase);  // all event expected
        Expression<Func<Event, bool>> exprTitleNone = x => x.Title.ToLower().Contains(titleNoneLowCase);      // No events        

        var expectedAll = fixture.Events.Where(exprTitleAll).ToList();
        var expectedAllCount = expectedAll.Count;

        // When
        var actualAll = fixture.FilterService
            .Reset()
            .AddCondition(exprTitleAll)
            .ApplyFilter(fixture.Events)
            .ToList();

        var actualAllCount = actualAll.Count;

        var actualNone = fixture.FilterService
            .Reset()
            .AddCondition(exprTitleNone)
            .ApplyFilter(fixture.Events)
            .ToList();

        // Then
        Assert.Equal(expectedAll, actualAll);
        Assert.All(actualAll, item => Assert.Contains(titleAll, item.Title, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(expectedAllCount, actualAllCount);
        Assert.Empty(actualNone);
    }

    [Trait(Category, Category_Filters)]
    [Theory]
    [InlineData(["event Title 1", 11])]
    [InlineData(["Event title 2", 11])]
    [InlineData(["event Title 3", 2])]
    public void PartialFilterByTitle_Success(string title, int expectedCount)
    {
        // Given
        var titleLowCase = title.ToLower();
        Expression<Func<Event, bool>> expr = x => x.Title.ToLower().Contains(titleLowCase);
        var expected = fixture.Events.Where(expr).ToList();

        // When        
        var actual = fixture.FilterService.Reset()
            .AddCondition(expr)
            .ApplyFilter(fixture.Events)
            .ToList();
        var actualCount = actual.Count;

        // Then
        Assert.Equal(expectedCount, expected.Count);
        Assert.All(actual, (item) => Assert.Contains(title, item.Title, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(expected, actual);
        Assert.Equal(expectedCount, actualCount);
    }

    public static readonly IEnumerable<object[]> Titles
        = [.. Enumerable.Range(1, TestAppDbContext.EventCount).Select(i => new object[] { $"Event Title {i}" })];

    [Trait(Category, Category_Filters)]
    [Theory]
    [MemberData(nameof(Titles))]
    public void IterationFilterByTitle_Success(string title)
    {
        // Givent
        var titleLowCase = title.ToLower();
        Expression<Func<Event, bool>> expression = x => x.Title.ToLower().Contains(titleLowCase);
        var expected = fixture.Events.Where(expression).ToList();
        var expectedCount = expected.Count;

        // When
        var actual = fixture.FilterService.Reset()
            .AddCondition(expression)
            .ApplyFilter(fixture.Events)
            .ToList();
        var actualCount = actual.Count;

        Assert.Equal(expectedCount, actualCount);
        Assert.Equal(expected, actual);
        Assert.All(actual, item => Assert.Contains(title, item.Title, StringComparison.OrdinalIgnoreCase));
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
    [Trait(Category, Category_Filters)]
    [Theory]
    [MemberData(nameof(StartDates))]
    public void FilterByStartDate_Success(DateTime startAt, int expectedCount)
    {
        // Given
        Expression<Func<Event, bool>> expression = x => x.StartAt >= startAt;
        var expected = fixture.Events.Where(expression).ToList();

        // When
        var actual = fixture.FilterService.Reset()
            .AddCondition(expression)
            .ApplyFilter(fixture.Events)
            .ToList();
        var actualCount = actual.Count;

        // Then
        Assert.Equal(expectedCount, expected.Count);
        Assert.Equal(expected, actual);
        Assert.All(actual, item => Assert.True(item.StartAt >= startAt));
        Assert.Equal(expectedCount, actualCount);
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
    [Trait(Category, Category_Filters)]
    [Theory]
    [MemberData(nameof(EndDates))]
    public void FilterByEndDate_Success(DateTime endAt, int expectedCount)
    {
        // Given
        endAt = endAt.AddDays(1).Date;
        Expression<Func<Event, bool>> expression = x => x.EndAt < endAt;
        var expected = fixture.Events.Where(expression).ToList();

        // When
        var actual = fixture.FilterService.Reset()
            .AddCondition(expression)
            .ApplyFilter(fixture.Events)
            .ToList();
        var actualCount = actual.Count;

        // Then
        Assert.Equal(expectedCount, expected.Count);
        Assert.Equal(expected, actual);
        Assert.All(actual, item => Assert.True(item.EndAt < endAt));
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

    [Trait(Category, Category_Filters)]
    [Theory]
    [MemberData(nameof(Combined))]
    public void CombinedFilter_Success(string? title, DateTime? startAt, DateTime? endAt, int expectedCount)
    {
        // Given
        var titleLowCase = title?.ToLower();
        endAt = endAt?.AddDays(1).Date;

        var expected = fixture.Events
            .Where(x => (string.IsNullOrEmpty(titleLowCase) || x.Title.ToLower().Contains(titleLowCase))
                && (startAt == null || x.StartAt >= startAt.Value)
                && (endAt == null || x.EndAt < endAt.Value))
            .ToList();

        Expression<Func<Event, bool>>? exprTitle = titleLowCase is null ? null : x => x.Title.ToLower().Contains(titleLowCase);
        Expression<Func<Event, bool>>? exprStartAt = startAt is null ? null : x => x.StartAt >= startAt.Value;
        Expression<Func<Event, bool>>? exprEndAt = endAt is null ? null : x => x.EndAt <= endAt;

        // When
        var filter = fixture.FilterService.Reset();

        if (exprTitle != null)
            filter.AddCondition(exprTitle);

        if (exprStartAt != null)
            filter.AddCondition(exprStartAt);

        if (exprEndAt != null)
            filter.AddCondition(exprEndAt);

        var actual = filter.ApplyFilter(fixture.Events).ToList();
        var actualCount = actual.Count;

        // Then
        Assert.Equal(expectedCount, expected.Count);

        Assert.Equal(expected, actual);

        Assert.All(actual, item =>
            {
                if (title != null)
                    Assert.Contains(title, item.Title, StringComparison.OrdinalIgnoreCase);

                if (startAt.HasValue)
                    Assert.True(item.StartAt >= startAt);

                if (endAt.HasValue)
                    Assert.True(item.EndAt < endAt);
            });

        Assert.Equal(expectedCount, actual.Count());
    }
}
