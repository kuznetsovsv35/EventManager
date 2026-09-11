using System.ComponentModel.DataAnnotations;
using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Data;
using EventManager.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Tests;

public class EventServiceTest(EventManagerTestContext context) : TestObjectBase, IClassFixture<EventManagerTestContext>
{
    ////////////////////////////////////////////////////////////////////////////////////////////
    /// Тесты управления событиями.
    ////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    ///  Успешное добавления события.
    /// </summary>
    [Trait(Category, Category_Service)]
    [Fact]
    public async Task CreateEvent_Success()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var title = "Simple event";
        var startAt = new DateTime(2026, 6, 28, 10, 0, 00);
        var endAt = new DateTime(2026, 6, 28, 10, 30, 00);
        var description = "Some event";
        var expectedCount = await dbContext.GetEvents().CountAsync() + 1;

        EventInputData inData = new()
        {
            Title = title,
            StartAt = startAt,
            EndAt = endAt,
            Description = description,
            TotalSeats = 15,
        };

        // When
        var outData = await eventService.CreateEventAsync(inData, CancellationToken.None);

        // Then
        var actualCount = await dbContext.GetEvents().CountAsync();
        Assert.Equal(expectedCount, actualCount);
        Assert.Equal(inData, outData);
    }

    /// <summary>
    /// Попытка создать событие по нулевой ссылке на входные данные.
    /// </summary>
    [Trait(Category, Category_Service)]
    [Fact]
    public async Task CreateEvent_Null()
    {
        // Given 
        await using var scope = context.CreateAsyncScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        // When

        // Then
        await Assert.ThrowsAnyAsync<ArgumentNullException>(async () => await eventService.CreateEventAsync(null!, CancellationToken.None));
    }

    /// <summary>
    /// Набор некорректных данных для теста попытки создать/обновить.
    /// </summary>
    public static readonly IEnumerable<object?[]> InvalidEventInputData = [
        [new EventInputData()], // Пустой заголовок, равные моменты начала и окончания события.
        [new EventInputData(){Title = null, EndAt = new DateTime(2026, 1, 15), StartAt = new DateTime(2026, 1, 14)}],
        [new EventInputData(){Title = "Title", EndAt = new DateTime(2026, 1, 14), StartAt = new DateTime(2026, 1, 15)}]
    ];
    /// <summary>
    /// Тест неудачных попыток создать/обновить некорректными данными.
    /// </summary>
    /// <param name="inputData"></param>
    [Trait(Category, Category_Service)]
    [Theory]
    [MemberData(nameof(InvalidEventInputData))]
    public async Task CreateEvent_Fail(EventInputData inputData)
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        // When

        // Then
        var ex = await Assert.ThrowsAnyAsync<ValidationException>(async () => await eventService.CreateEventAsync(inputData, CancellationToken.None));
        Assert.NotNull(ex?.ValidationResult?.MemberNames);
        Assert.NotEmpty(ex.ValidationResult.MemberNames);
    }

    /// <summary>
    /// Получить все события с представлением в DTO (выход).
    /// </summary>
    [Trait(Category, Category_Service)]
    [Fact]
    public async Task GetAllEvents_Success()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var expected = await dbContext
            .GetEvents()
            .OrderByDescending(e => e.StartAt)
            .Select(x => x.ToOutputData()).ToListAsync();

        // When
        var actual = eventService.GetAllEvents().ToList();

        // Then
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Получить событие с существующим ID.
    /// </summary>
    [Trait(Category, Category_Service)]
    [Fact]
    public async Task GetEventByID_Success()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var requestedId = await context.GetRandomEventId(CancellationToken.None);
        var @event = (await dbContext
            .GetEvents(x => x.Id == requestedId)
            .SingleAsync(CancellationToken.None)).ToOutputData();

        // When
        var foundEvent = await eventService.GetEventAsync(requestedId, CancellationToken.None);

        // Then
        Assert.NotNull(foundEvent);
        Assert.Equal(requestedId, foundEvent.Id);
    }

    /// <summary>
    /// Тест неудачной попытки получить событие по несуществующему ID.
    /// </summary>
    [Trait(Category, Category_Service)]
    [Fact]
    public async Task GetEventByID_Fail()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var requestedId = Guid.NewGuid();

        // When

        // Then
        var ex = await Assert.ThrowsAsync<EventNotFoundException>(async () => await eventService.GetEventAsync(requestedId, CancellationToken.None));
        Assert.Equal(requestedId, ex.ObjectKey);
    }

    /// <summary>
    /// Тест успешного обновление события.
    /// </summary>
    [Trait(Category, Category_Service)]
    [Fact]
    public async Task UpdateEvent_Success()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var requestedId = await context.GetRandomEventId(CancellationToken.None);

        EventInputData inputData = new()
        {
            Title = "Title updated",
            StartAt = new DateTime(1976, 1, 15, 15, 34, 0),
            EndAt = new DateTime(1976, 1, 15, 16, 34, 0),
            Description = "Description updated",
            TotalSeats = 100,
        };

        // When
        var outData = await eventService.UpdateEventAsync(requestedId, inputData, CancellationToken.None);

        // Then
        Assert.NotNull(outData);
        Assert.Equal(requestedId, outData.Id);
        Assert.Equal(inputData, outData);
    }

    /// <summary>
    /// Тест неудачного обновление события по несуществующему ID.
    /// </summary>
    [Trait(Category, Category_Service)]
    [Fact]
    public async Task UpdateEventByID_Fail()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        EventInputData inputData = new()
        {
            Title = "Title updated",
            StartAt = new DateTime(1976, 1, 15, 15, 34, 0),
            EndAt = new DateTime(1976, 1, 15, 16, 34, 0),
            Description = "Description updated"
        };

        var requestedId = Guid.NewGuid();

        // When

        // Then
        var ex = await Assert.ThrowsAsync<EventNotFoundException>(async () => await eventService.UpdateEventAsync(requestedId, inputData, CancellationToken.None));
        Assert.Equal(requestedId, ex.ObjectKey);
    }

    /// <summary>
    /// Тест неудачного обновления с некорректными входными данными.
    /// </summary>
    [Trait(Category, Category_Service)]
    [Theory]
    [MemberData(nameof(InvalidEventInputData))]
    public async Task UpdateEven_Fail(EventInputData inputData)
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var requestedId = await context.GetRandomEventId(CancellationToken.None);

        // When

        // Then
        await Assert.ThrowsAsync<ValidationException>(async () => await eventService.UpdateEventAsync(requestedId, inputData, CancellationToken.None));
    }

    /// <summary>
    /// Тест успешного удаления события.
    /// </summary>
    [Trait(Category, Category_Service)]
    [Fact]
    public async Task DeleteEvent_Success()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var requestedId = await context.GetRandomEventId(CancellationToken.None);
        var requestedEvent = await dbContext.GetEvents(x => x.Id == requestedId).SingleAsync(CancellationToken.None);
        var expectedEvent = requestedEvent.ToOutputData();

        // When
        var deletedEvent = await eventService.DeleteEventAsync(requestedId, CancellationToken.None);

        // Then
        var foundEvent = await dbContext.GetEvents(e => e.Id == requestedId).SingleOrDefaultAsync(CancellationToken.None);
        Assert.Equal(expectedEvent, deletedEvent);
        Assert.Null(foundEvent);
    }

    /// <summary>
    /// Тест неуспешного удаления события по несуществующему ID.
    /// </summary>
    [Trait(Category, Category_Service)]
    [Fact]
    public async Task DeleteEventByID_Fail()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var requestedId = Guid.NewGuid();

        // When

        // Then
        var ex = await Assert.ThrowsAsync<EventNotFoundException>(() => eventService.DeleteEventAsync(requestedId, CancellationToken.None));
        Assert.Equal(requestedId, ex.ObjectKey);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    /// Тесты фильтров в комплексе.
    ////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    ///  Тест применения простейшего фильтра по заголовку.
    /// </summary>
    [Trait(Category, Category_Filters)]
    [Fact]
    public async Task SimpleFilterByTitle_Success()
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        const string titleAll = "Event title";  // all event expected
        const string titleNone = "AbcDeF";      // No events

        var expectedAll = await dbContext
            .GetEvents(x => x.Title.Contains(titleAll, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.StartAt)
            .Select(x => x.ToOutputData())
            .ToListAsync();

        // When
        var actualAll = await eventService
            .GetEvents(new() { Title = titleAll })
            .ToListAsync();
        var actualNone = await eventService
            .GetEvents(new() { Title = titleNone })
            .ToListAsync();

        // Then
        Assert.Equal(expectedAll, actualAll);
        Assert.All(actualAll, item => Assert.Contains(titleAll, item.Title, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(expectedAll.Count, actualAll.Count);
        Assert.Empty(actualNone);
    }

    public static readonly IEnumerable<object[]> Titles
        = [.. Enumerable.Range(1, TestAppDbContext.EventCount).Select(i => new object[] { $"eVeNt TiTlE {i}" })];

    /// <summary>
    /// Тест фильтров по заголовку.
    /// </summary>
    /// <param name="title"></param>
    [Trait(Category, Category_Filters)]
    [Theory]
    [MemberData(nameof(Titles))]
    public async Task IterationFilterByTitle_Success(string title)
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var expected = await dbContext
            .GetEvents(x => x.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.StartAt)
            .Select(x => x.ToOutputData())
            .ToListAsync();

        var actual = await eventService
            .GetEvents(new() { Title = title })
            .ToListAsync();

        Assert.All(actual, item => Assert.Contains(title, item.Title, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(expected, actual);
        Assert.True(actual.Any());
    }

    /// <summary>
    /// Тестовый набор дат для начала.
    /// </summary>
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

    /// <summary>
    /// Тест фильтрации по началу события.
    /// </summary>
    /// <param name="startAt"></param>
    [Trait(Category, Category_Filters)]
    [Theory]
    [MemberData(nameof(StartDates))]
    public async Task FilterByStartDate_Success(DateTime startAt)
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var expected = await dbContext
            .GetEvents(x => x.StartAt >= startAt)
            .OrderByDescending(e => e.StartAt)
            .Select(x => x.ToOutputData())
            .ToListAsync();

        // When
        var actual = await eventService
            .GetEvents(new() { From = startAt })
            .ToListAsync();

        // Then
        Assert.Equal(expected, actual);
        Assert.All(actual, item => Assert.True(item.StartAt >= startAt));
    }

    /// <summary>
    /// Тестовый набор дат для фильтров окончания.
    /// </summary>
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

    /// <summary>
    /// Тест фильтра по окончанию события.
    /// </summary>
    /// <param name="endAt"></param>
    [Trait(Category, Category_Filters)]
    [Theory]
    [MemberData(nameof(EndDates))]
    public async Task FilterByEndDate_Success(DateTime endAt)
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var endDate = endAt.AddDays(1).Date;
        var expected = await dbContext
            .GetEvents(x => x.EndAt < endDate)
            .OrderByDescending(e => e.StartAt)
            .Select(x => x.ToOutputData())
            .ToListAsync();

        // When
        var actual = await eventService
            .GetEvents(new() { To = endAt })
            .ToListAsync();

        // Then
        Assert.Equal(expected, actual);
        Assert.All(actual, item => Assert.True(item.EndAt < endDate));
    }

    /// <summary>
    /// Тестовый набор данных для тестов комбинированных фильтров.
    /// </summary>
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

    /// <summary>
    /// Тест комбинированных фильтров.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="startAt"></param>
    /// <param name="endAt"></param>
    [Trait(Category, Category_Filters)]
    [Theory]
    [MemberData(nameof(Combined))]
    public async Task CombinedFilter_Success(string? title, DateTime? startAt, DateTime? endAt)
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var endDate = endAt?.AddDays(1).Date;

        var expected = await dbContext
            .GetEvents(x => (string.IsNullOrEmpty(title) || x.Title.ToLower().Contains(title, StringComparison.OrdinalIgnoreCase))
                && (startAt == null || x.StartAt >= startAt.Value)
                && (endDate == null || x.EndAt < endDate.Value))
            .OrderByDescending(e => e.StartAt)
            .Select(x => x.ToOutputData())
            .ToListAsync();

        // When
        var actual = await eventService
            .GetEvents(new() { Title = title, From = startAt, To = endAt })
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
                    Assert.True(item.EndAt < endDate);
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
    [Trait(Category, Category_Paginator)]
    [Theory]
    [InlineData([1, 10, 3, 10])]
    [InlineData([3, 10, 3, 10])]
    [InlineData([4, 10, 3, 0])]
    [InlineData([2, 7, 5, 7])]
    [InlineData([5, 7, 5, 2])]
    [InlineData([1, 30, 1, 30])]
    public async Task PaginateResult_Success(int page, int pageSize, int expectedPageCount, int expectedPageSize)
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var allValues = eventService
            .GetAllEvents()
            .ToList();
        
        var expectedTotalCount = allValues.Count;

        var expectedValues = allValues
            .OrderByDescending(e => e.StartAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // When
        var pageResult = await eventService.GetEvents(null, new() { CurrentPage = page, PageSize = pageSize }, CancellationToken.None);

        // Then
        Assert.Equal(expectedPageCount, pageResult.PageCount);
        Assert.Equal(page, pageResult.PageNumber);
        Assert.Equal(expectedPageSize, pageResult.PageSize);
        Assert.Equal(expectedTotalCount, pageResult.TotalCount);
        Assert.Equal(expectedPageSize, pageResult.Values.Count());
        Assert.Equal(expectedValues, pageResult.Values);
    }
}