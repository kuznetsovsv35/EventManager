using EventManager.Application.DataTransfer;
using EventManager.Infrastructure;

namespace EventManager.Tests;

/// <summary>
/// Тест разбивки на чираницы.
/// </summary>
/// <param name="fixture"></param>
public class PaginatorTest(EventServiceFixture fixture) : IClassFixture<EventServiceFixture>
{
    /// <summary>
    /// Тест валидации параметров на страницы.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    [Theory]
    [InlineData([-1, 10])]
    [InlineData([0, 10])]
    [InlineData([1, -20])]
    [InlineData([1, 0])]
    public void ValidateParameters_Fail(int page, int pageSize)
    {
        // Given
        PageParams p = new() { CurrentPage = page, PageSize = pageSize };
    
        // When
    
        // Then
        Assert.Throws<PaginatorParamException>(() => fixture.AppService.GetEvents(null, p));
    }

    /// <summary>
    /// Тест разбизвки на страницы.
    /// </summary>
    /// <param name="page">Запращиваемая страница.</param>
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
        var expectedTotalCount = TestAppDbContext.TestData.Length;
        var expectedValues = TestAppDbContext.TestData
            .Skip((page -1) * pageSize)
            .Take(pageSize)
            .Select(x => x.ToOutputData());

        var pageResult = fixture.AppService.GetEvents(null, new(){ CurrentPage = page, PageSize = pageSize });
        
        Assert.Equal(expectedPageCount, pageResult.PageCount);
        Assert.Equal(page, pageResult.PageNumber);
        Assert.Equal(expectedPageSize, pageResult.PageSize);
        Assert.Equal(expectedTotalCount, pageResult.TotalCount);
        Assert.Equal(expectedPageSize, pageResult.Values.Count());
        Assert.Equal(expectedValues, pageResult.Values);
    }
}