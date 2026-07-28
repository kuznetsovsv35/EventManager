using System.Linq.Expressions;
using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Tests;

/// <summary>
/// Модульные тесты сервиса фильтрации.
/// </summary>
/// <param name="fixture"></param>
public class FilterEventTest(EventManagerTestContext context) : TestObjectBase, IClassFixture<EventManagerTestContext>
{
    [Trait(Category, Category_Filters)]
    [Fact]
    public async Task Reset_Success()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var filterService = scope.ServiceProvider.GetRequiredService<IFilter<Event>>();

        // When
        var filter = filterService.Reset();

        // Then
        Assert.Null(filter.Expression);
    }

    [Trait(Category, Category_Filters)]
    [Fact]
    public async Task AddNullCondition_Fail()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var filterService = scope.ServiceProvider.GetRequiredService<IFilter<Event>>();

        // Then
        var ex = Assert.Throws<ArgumentNullException>(() => filterService.AddCondition(null!));
        Assert.NotNull(ex.ParamName);
        Assert.NotEmpty(ex.ParamName);
    }

    [Trait(Category, Category_Filters)]
    [Fact]
    public async Task SimpleFilterByTitle_Success()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var filterService = scope.ServiceProvider.GetRequiredService<IFilter<Event>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        const string titleAll = "Event title";
        const string titleNone = "AbcDeF";

        Expression<Func<Event, bool>> exprTitleAll = x => x.Title.Contains(titleAll, StringComparison.OrdinalIgnoreCase);  // all event expected
        Expression<Func<Event, bool>> exprTitleNone = x => x.Title.Contains(titleNone, StringComparison.OrdinalIgnoreCase);      // No events        

        var expectedAll = await dbContext
            .GetEvents(exprTitleAll)
            .Select(x => x.ToOutputData())
            .ToListAsync();

        // When
        var actualAll = await filterService
            .Reset()
            .AddCondition(exprTitleAll)
            .ApplyFilter(dbContext.GetEvents())
            .Select(x => x.ToOutputData())
            .ToListAsync();


        var actualNone = await filterService
            .Reset()
            .AddCondition(exprTitleNone)
            .ApplyFilter(dbContext.GetEvents())
            .Select(x => x.ToOutputData())
            .ToListAsync();

        // Then
        Assert.Equal(expectedAll, actualAll);
        Assert.All(actualAll, item => Assert.Contains(titleAll, item.Title, StringComparison.OrdinalIgnoreCase));
        Assert.Empty(actualNone);
    }

    [Trait(Category, Category_Filters)]
    [Theory]
    [InlineData(["event Title 1"])]
    [InlineData(["Event title 2"])]
    [InlineData(["event Title 3"])]
    public async Task PartialFilterByTitle_Success(string title)
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var filterService = scope.ServiceProvider.GetRequiredService<IFilter<Event>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        Expression<Func<Event, bool>> expr = x => x.Title.Contains(title, StringComparison.OrdinalIgnoreCase);
        var expected = await dbContext
            .GetEvents(expr)
            .Select(x => x.ToOutputData())
            .ToListAsync();

        // When        
        var actual = await filterService.Reset()
            .AddCondition(expr)
            .ApplyFilter(dbContext.GetEvents())
            .Select(x => x.ToOutputData())
            .ToListAsync();

        // Then
        Assert.All(actual, (item) => Assert.Contains(title, item.Title, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(expected, actual);
    }

    public static readonly IEnumerable<object[]> Titles
        = [.. Enumerable.Range(1, TestAppDbContext.EventCount).Select(i => new object[] { $"Event Title {i}" })];

    [Trait(Category, Category_Filters)]
    [Theory]
    [MemberData(nameof(Titles))]
    public async Task IterationFilterByTitle_Success(string title)
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var filterService = scope.ServiceProvider.GetRequiredService<IFilter<Event>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        Expression<Func<Event, bool>> expression = x => x.Title.Contains(title, StringComparison.OrdinalIgnoreCase);
        var expected = await dbContext
            .GetEvents(expression)
            .Select(x => x.ToOutputData())
            .ToListAsync();

        // When
        var actual = await filterService.Reset()
            .AddCondition(expression)
            .ApplyFilter(dbContext.GetEvents())
            .Select(x => x.ToOutputData())
            .ToListAsync();

        Assert.Equal(expected, actual);
        Assert.All(actual, item => Assert.Contains(title, item.Title, StringComparison.OrdinalIgnoreCase));
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
    [Trait(Category, Category_Filters)]
    [Theory]
    [MemberData(nameof(StartDates))]
    public async Task FilterByStartDate_Success(DateTime startAt)
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var filterService = scope.ServiceProvider.GetRequiredService<IFilter<Event>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        Expression<Func<Event, bool>> expression = x => x.StartAt >= startAt;
        var expected = await dbContext
            .GetEvents(expression)
            .Select(x => x.ToOutputData())
            .ToListAsync();

        // When
        var actual = await filterService.Reset()
            .AddCondition(expression)
            .ApplyFilter(dbContext.GetEvents())
            .Select(x => x.ToOutputData())
            .ToListAsync();

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
    [Trait(Category, Category_Filters)]
    [Theory]
    [MemberData(nameof(EndDates))]
    public async Task FilterByEndDate_Success(DateTime endAt)
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var filterService = scope.ServiceProvider.GetRequiredService<IFilter<Event>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        endAt = endAt.AddDays(1).Date;
        Expression<Func<Event, bool>> expression = x => x.EndAt < endAt;
        var expected = await dbContext
            .GetEvents(expression)
            .Select(x => x.ToOutputData())
            .ToListAsync();

        // When
        var actual = await filterService.Reset()
            .AddCondition(expression)
            .ApplyFilter(dbContext.GetEvents())
            .Select(x => x.ToOutputData())
            .ToListAsync();

        // Then
        Assert.Equal(expected, actual);
        Assert.All(actual, item => Assert.True(item.EndAt < endAt));
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

    [Trait(Category, Category_Filters)]
    [Theory]
    [MemberData(nameof(Combined))]
    public async Task CombinedFilter_Success(string? title, DateTime? startAt, DateTime? endAt)
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var filterService = scope.ServiceProvider.GetRequiredService<IFilter<Event>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        endAt = endAt?.AddDays(1).Date;
        var expected = await dbContext
            .GetEvents(
                x => (string.IsNullOrEmpty(title) || x.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
                && (startAt == null || x.StartAt >= startAt.Value)
                && (endAt == null || x.EndAt < endAt.Value))
            .Select(x => x.ToOutputData())
            .ToListAsync();

        Expression<Func<Event, bool>>? exprTitle = title is null ? null : x => x.Title.Contains(title, StringComparison.OrdinalIgnoreCase);
        Expression<Func<Event, bool>>? exprStartAt = startAt is null ? null : x => x.StartAt >= startAt.Value;
        Expression<Func<Event, bool>>? exprEndAt = endAt is null ? null : x => x.EndAt <= endAt;

        // When
        var filter = filterService.Reset();

        if (exprTitle != null)
            filter.AddCondition(exprTitle);

        if (exprStartAt != null)
            filter.AddCondition(exprStartAt);

        if (exprEndAt != null)
            filter.AddCondition(exprEndAt);

        var actual = await filter
            .ApplyFilter(dbContext.GetEvents())
            .Select(x => x.ToOutputData())
            .ToListAsync();

        // Then
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
    }
}
