using System.ComponentModel.DataAnnotations;
using EventManager.Application.DataTransfer;

namespace EventManager.Tests;

public class EventServiceTest(EventServiceFixture fixture) : TraitAttributes, IClassFixture<EventServiceFixture>
{
    ////////////////////////////////////////////////////////////////////////////////////////////
    /// Тесты управления событиями.
    ////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    ///  Успешное добавления события.
    /// </summary>
    [Trait(Category, Category_Service)]
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
        var outData = fixture.EventService.CreateEvent(inData);

        // Then
        var actualCount = fixture.Events.Count();
        Assert.Equal(expectedCount, actualCount);
        Assert.Equal(inData, outData);
    }

    /// <summary>
    /// Попытка создать событие по нулвой ссцлке на входне данные.
    /// </summary>
    [Trait(Category, Category_Service)]
    [Fact]
    public void CreateEvent_Null()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => fixture.EventService.CreateEvent(null!));
    }

    /// <summary>
    /// Набор некоррктных данных для теста попытки создать/обновить.
    /// </summary>
    public static readonly IEnumerable<object?[]> InvalidEventInputData = [
        [new EventInputData()], // Пустой заголовок, равные моменты начала и окончания события.
        [new EventInputData(){Title = null, EndAt = new DateTime(2026, 1, 15), StartAt = new DateTime(2026, 1, 14)}],
        [new EventInputData(){Title = "Title", EndAt = new DateTime(2026, 1, 14), StartAt = new DateTime(2026, 1, 15)}]
    ];
    /// <summary>
    /// Тест неудачных попыток создать/обновить некоректными данными.
    /// </summary>
    /// <param name="inputData"></param>
    [Trait(Category, Category_Service)]
    [Theory]
    [MemberData(nameof(InvalidEventInputData))]
    void CreateEvent_Fail(EventInputData inputData)
    {
        var ex = Assert.Throws<ValidationException>(() => fixture.EventService.CreateEvent(inputData));
        Assert.NotNull(ex?.ValidationResult?.MemberNames);
        Assert.NotEmpty(ex.ValidationResult.MemberNames);
    }

    /// <summary>
    /// Получить все события с представлением в DTO (выход).
    /// </summary>
    [Trait(Category, Category_Service)]
    [Fact]
    public void GetAllEvents_Success()
    {
        // Given
        var expected = fixture.Events.Select(x => x.ToOutputData());

        // When
        var actual = fixture.EventService.GetAllEvents();

        // Then
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Получть событие с существующим ID.
    /// </summary>
    [Trait(Category, Category_Service)]
    [Fact]
    public void GetEventByID_Success()
    {
        // Given
        var firstEvent = fixture.Events.First();
        var requestedId = firstEvent.Id;

        // When
        var foundEvent = fixture.EventService.GetEvent(requestedId);

        // Then
        Assert.NotNull(foundEvent);
        Assert.Equal(requestedId, foundEvent.Id);
    }

    /// <summary>
    /// Тест неудачнай попытки получить событие по несуществуюшему ID.
    /// </summary>
    [Trait(Category, Category_Service)]
    [Fact]
    public void GetEventByID_Fail()
    {
        // Given
        var requestedId = Guid.Empty;

        // When
        var foundEvent = fixture.EventService.GetEvent(requestedId);

        // Then
        Assert.Null(foundEvent);
    }

    /// <summary>
    /// Тест успошнго обновление события.
    /// </summary>
    [Trait(Category, Category_Service)]
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
        var outData = fixture.EventService.UpdateEvent(requestedId, inputData);

        // Then
        Assert.NotNull(outData);
        Assert.Equal(requestedId, outData.Id);
        Assert.Equal(inputData, outData);
    }

    /// <summary>
    /// Тест неудачноо обновление обытия по несуществующему ID.
    /// </summary>
    [Trait(Category, Category_Service)]
    [Fact]
    public void UpdateEventByID_Fail()
    {
        // Given
        EventInputData inputData = new()
        {
            Title = "Title updated",
            StartAt = new DateTime(1976, 1, 15, 15, 34, 0),
            EndAt = new DateTime(1976, 1, 15, 16, 34, 0),
            Description = "Description updated"
        };

        var requestedId = Guid.Empty;

        // When
        var outData = fixture.EventService.UpdateEvent(requestedId, inputData);

        // Then
        Assert.Null(outData);
    }

    /// <summary>
    /// Тест неудачного обновления с некоректными входными данными.
    /// </summary>
    [Trait(Category, Category_Service)]
    [Theory]
    [MemberData(nameof(InvalidEventInputData))]
    public void UpdateEven_Fail(EventInputData inputData)
    {
        // Given
        var requestedId = fixture.Events.Last().Id;

        // When

        // Then
        Assert.Throws<ValidationException>(() => fixture.EventService.UpdateEvent(requestedId, inputData));
    }

    /// <summary>
    /// Тест успешного удаления события.
    /// </summary>
    [Trait(Category, Category_Service)]
    [Fact]
    public void DeleteEvent_Success()
    {
        // Given
        var requestedEvent = fixture.Events.First();
        var requestedId = requestedEvent.Id;
        var expectedEvent = requestedEvent.ToOutputData();
        var expectedCount = fixture.Events.Count() - 1;

        // When
        var deletedEvent = fixture.EventService.DeleteEvent(requestedId);

        // Then
        var actualCount = fixture.Events.Count();
        var foundEvent = fixture.Events.FirstOrDefault(e => e.Id == requestedId);
        Assert.Equal(expectedCount, actualCount);
        Assert.Equal(expectedEvent, deletedEvent);
        Assert.Null(foundEvent);
    }

    /// <summary>
    /// Тест неуспешного удаления события по несуществующему ID.
    /// </summary>
    [Trait(Category, Category_Service)]
    [Fact]
    public void DeleteEventByID_Fail()
    {
        // Given
        var requestedId = Guid.Empty;
        var expectedCount = fixture.Events.Count();

        // When
        var deletedEvent = fixture.EventService.DeleteEvent(requestedId);

        // Then
        var actualCount = fixture.Events.Count();
        Assert.Equal(expectedCount, actualCount);
        Assert.Null(deletedEvent);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    /// Тесты фильтров в комплексе.
    ////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    ///  Тест применения простейшего фильтра по заголовку.
    /// </summary>
    [Trait(Category, Category_Filters)]
    [Fact]
    public void SimpleFilterByTitle_Success()
    {
        // Given
        const string titleAll = "Event title";  // all event expected
        const string titleNone = "AbcDeF";      // No events
        var titleAllLowCase = titleAll.ToLower();
        var titleNoneLowcase = titleNone.ToLower();

        var expectedAll = fixture.Events
            .Where(x => x.Title.ToLower().Contains(titleAllLowCase))
            .Select(x => x.ToOutputData())
            .ToList();

        // When
        var actualAll = fixture.EventService.GetEvents(new() { Title = titleAll }).ToList();
        var actualNone = fixture.EventService.GetEvents(new() { Title = titleNone }).ToList();

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
    public void IterationFilterByTitle_Success(string title)
    {
        var titleLowCase = title.ToLower();
        var expected = fixture.Events
            .Where(x => x.Title.ToLower().Contains(titleLowCase))
            .Select(x => x.ToOutputData())
            .ToList();

        var actual = fixture.EventService.GetEvents(new() { Title = title }).ToList();

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
    /// Тест фильтрации по нвчалу события.
    /// </summary>
    /// <param name="startAt"></param>
    [Trait(Category, Category_Filters)]
    [Theory]
    [MemberData(nameof(StartDates))]
    public void FilterByStartDate_Success(DateTime startAt)
    {
        // Given
        var expected = fixture.Events
            .Where(x => x.StartAt >= startAt)
            .Select(x => x.ToOutputData())
            .ToList();

        // When
        var actual = fixture.EventService.GetEvents(new() { From = startAt }).ToList();

        // Then
        Assert.Equal(expected, actual);
        Assert.All(actual, item => Assert.True(item.StartAt >= startAt));
    }

    /// <summary>
    /// Тестовы набор дат для фильтров окончания.
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
    public void FilterByEndDate_Success(DateTime endAt)
    {
        var endDate = endAt.AddDays(1).Date;
        // Given        
        var expected = fixture.Events.Where(x => x.EndAt < endDate).Select(x => x.ToOutputData()).ToList();

        // When
        var actual = fixture.EventService.GetEvents(new() { To = endAt }).ToList();

        // Then
        Assert.Equal(expected, actual);
        Assert.All(actual, item => Assert.True(item.EndAt < endDate));
    }

    /// <summary>
    /// Тестовый набор данных для тетовв комбинированных фильтров.
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
    public void CombinedFilter_Success(string? title, DateTime? startAt, DateTime? endAt)
    {
        // Given
        var endDate = endAt?.AddDays(1).Date;
        var titleLowCase = title?.ToLower();

        var expected = fixture.Events
            .Where(x => (string.IsNullOrEmpty(titleLowCase) || x.Title.ToLower().Contains(titleLowCase))
                && (startAt == null || x.StartAt >= startAt.Value)
                && (endDate == null || x.EndAt < endDate.Value))
            .Select(x => x.ToOutputData())
            .ToList();

        // When
        var actual = fixture.EventService.GetEvents(new() { Title = title, From = startAt, To = endAt }).ToList();

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
    public void PaginateResult_Success(int page, int pageSize, int expectedPageCount, int expectedPageSize)
    {
        // Given
        var allValues = fixture.EventService.GetAllEvents().ToList();
        var expectedTotalCount = allValues.Count;

        var expectedValues = allValues
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // When
        var pageResult = fixture.EventService.GetEvents(null, new() { CurrentPage = page, PageSize = pageSize });

        // Then
        Assert.Equal(expectedPageCount, pageResult.PageCount);
        Assert.Equal(page, pageResult.PageNumber);
        Assert.Equal(expectedPageSize, pageResult.PageSize);
        Assert.Equal(expectedTotalCount, pageResult.TotalCount);
        Assert.Equal(expectedPageSize, pageResult.Values.Count());
        Assert.Equal(expectedValues, pageResult.Values);
    }
}