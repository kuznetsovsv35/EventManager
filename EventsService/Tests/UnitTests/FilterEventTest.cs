using System.Linq.Expressions;
using Common.Data;
using Common.Tests;
using EventsService.Application.DTO;
using EventsService.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

/// <summary>
/// Модульные тесты сервиса фильтрации.
/// </summary>
/// <param name="fixture"></param>
public class FilterEventTest(TestContext context) : IClassFixture<TestContext>
{
    [Trait(nameof(Category), Category.Filters)]
    [Fact]
    public async Task Reset_Success()
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var filterService = scope.ServiceProvider.GetRequiredService<IFilter<Event>>();

        // When
        var filter = filterService.Reset();

        // Then
        Assert.Null(filter.Expression);
    }

    [Trait(nameof(Category), Category.Filters)]
    [Fact]
    public async Task AddNullCondition_Fail()
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var filterService = scope.ServiceProvider.GetRequiredService<IFilter<Event>>();

        // Then
        var ex = Assert.Throws<ArgumentNullException>(() => filterService.AddCondition(null!));
        Assert.NotNull(ex.ParamName);
        Assert.NotEmpty(ex.ParamName);
    }

    [Trait(nameof(Category), Category.Filters)]
    [Fact]
    public async Task SimpleFilterByTitle_Success()
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var filterService = scope.ServiceProvider.GetRequiredService<IFilter<Event>>();

        const string titleAll = "Event title";
        const string titleNone = "AbcDeF";

        Expression<Func<Event, bool>> exprTitleAll = x => x.Title.Contains(titleAll, StringComparison.OrdinalIgnoreCase);  // all event expected
        Expression<Func<Event, bool>> exprTitleNone = x => x.Title.Contains(titleNone, StringComparison.OrdinalIgnoreCase);      // No events        

        var expectedAll = context
            .Events
            .AsQueryable()
            .Where(exprTitleAll)
            .Select(x => x.ToOutputData())
            .ToList();

        // When
        var actualAll = filterService
            .Reset()
            .AddCondition(exprTitleAll)
            .ApplyFilter(context.Events.AsQueryable())
            .Select(x => x.ToOutputData())
            .ToList();


        var actualNone = filterService
            .Reset()
            .AddCondition(exprTitleNone)
            .ApplyFilter(context.Events.AsQueryable())
            .Select(x => x.ToOutputData())
            .ToList();

        // Then
        Assert.Equal(expectedAll, actualAll);
        Assert.All(actualAll, item => Assert.Contains(titleAll, item.Title, StringComparison.OrdinalIgnoreCase));
        Assert.Empty(actualNone);
    }

    [Trait(nameof(Category), Category.Filters)]
    [Theory]
    [InlineData(["event Title 1"])]
    [InlineData(["Event title 2"])]
    [InlineData(["event Title 3"])]
    public async Task PartialFilterByTitle_Success(string title)
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var filterService = scope.ServiceProvider.GetRequiredService<IFilter<Event>>();

        Expression<Func<Event, bool>> expr = x => x.Title.Contains(title, StringComparison.OrdinalIgnoreCase);
        var expected = context
            .Events
            .AsQueryable()
            .Where(expr)
            .Select(x => x.ToOutputData())
            .ToList();

        // When        
        var actual = filterService.Reset()
            .AddCondition(expr)
            .ApplyFilter(context.Events.AsQueryable())
            .Select(x => x.ToOutputData())
            .ToList();

        // Then
        Assert.All(actual, (item) => Assert.Contains(title, item.Title, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(expected, actual);
    }

    public static readonly IEnumerable<object[]> Titles
        = [.. Enumerable.Range(1, TestContext.EventCount).Select(i => new object[] { $"Event Title {i}" })];

    [Trait(nameof(Category), Category.Filters)]
    [Theory]
    [MemberData(nameof(Titles))]
    public async Task IterationFilterByTitle_Success(string title)
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var filterService = scope.ServiceProvider.GetRequiredService<IFilter<Event>>();

        Expression<Func<Event, bool>> expression = x => x.Title.Contains(title, StringComparison.OrdinalIgnoreCase);
        var expected = context
            .Events
            .AsQueryable()
            .Where(expression)
            .Select(x => x.ToOutputData())
            .ToList();

        // When
        var actual = filterService.Reset()
            .AddCondition(expression)
            .ApplyFilter(context.Events.AsQueryable())
            .Select(x => x.ToOutputData())
            .ToList();

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
    [Trait(nameof(Category), Category.Filters)]
    [Theory]
    [MemberData(nameof(StartDates))]
    public async Task FilterByStartDate_Success(DateTime startAt)
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var filterService = scope.ServiceProvider.GetRequiredService<IFilter<Event>>();

        Expression<Func<Event, bool>> expression = x => x.StartAt >= startAt;
        var expected = context
            .Events
            .AsQueryable()
            .Where(expression)
            .Select(x => x.ToOutputData())
            .ToList();

        // When
        var actual = filterService.Reset()
            .AddCondition(expression)
            .ApplyFilter(context.Events.AsQueryable())
            .Select(x => x.ToOutputData())
            .ToList();

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
    [Trait(nameof(Category), Category.Filters)]
    [Theory]
    [MemberData(nameof(EndDates))]
    public async Task FilterByEndDate_Success(DateTime endAt)
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var filterService = scope.ServiceProvider.GetRequiredService<IFilter<Event>>();

        endAt = endAt.AddDays(1).Date;
        Expression<Func<Event, bool>> expression = x => x.EndAt < endAt;
        var expected = context
            .Events
            .AsQueryable()
            .Where(expression)
            .Select(x => x.ToOutputData())
            .ToList();

        // When
        var actual = filterService.Reset()
            .AddCondition(expression)
            .ApplyFilter(context.Events.AsQueryable())
            .Select(x => x.ToOutputData())
            .ToList();

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

    [Trait(nameof(Category), Category.Filters)]
    [Theory]
    [MemberData(nameof(Combined))]
    public async Task CombinedFilter_Success(string? title, DateTime? startAt, DateTime? endAt)
    {
        // Given
        var serviceProvider = context.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var filterService = scope.ServiceProvider.GetRequiredService<IFilter<Event>>();

        endAt = endAt?.AddDays(1).Date;
        var expected = context
            .Events
            .AsQueryable()
            .Where(
                x => (string.IsNullOrEmpty(title) || x.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
                && (startAt == null || x.StartAt >= startAt.Value)
                && (endAt == null || x.EndAt < endAt.Value))
            .Select(x => x.ToOutputData())
            .ToList();

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

        var actual = filter
            .ApplyFilter(context.Events.AsQueryable())
            .Select(x => x.ToOutputData())
            .ToList();

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
