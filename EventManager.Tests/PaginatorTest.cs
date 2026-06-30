using EventManager.Application.DataTransfer;
using EventManager.Infrastructure;

namespace EventManager.Tests;

/// <summary>
/// Тест разбивки на страницы.
/// </summary>
/// <param name="fixture"></param>
public class PaginatorTest(PaginatorFixture fixture) : IClassFixture<PaginatorFixture>
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
    
        // When
    
        // Then
        Assert.Throws<PaginatorParamException>(
            () => fixture.Paginator.Paginate(fixture.Events, page, pageSize, x => x));
    }

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
        var expectedTotalCount = fixture.Events.Count();
        var expectedValues = fixture.Events
            .Skip((page -1) * pageSize)
            .Take(pageSize);

        // When
        var pageResult = fixture.Paginator.Paginate(fixture.Events, page, pageSize, x => x);
        
        // Then
        Assert.Equal(expectedPageCount, pageResult.PageCount);
        Assert.Equal(page, pageResult.PageNumber);
        Assert.Equal(expectedPageSize, pageResult.PageSize);
        Assert.Equal(expectedTotalCount, pageResult.TotalCount);
        Assert.Equal(expectedPageSize, pageResult.Values.Count());
        Assert.Equal(expectedValues, pageResult.Values);
    }
}