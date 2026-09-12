using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.IntegrationTests;

public class PaginatorTest : DataTest
{
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
        await using var dbContext = CreateDbContext();
        IPaginator<Event> paginator = new PaginateService<Event>();

        var expectedTotalCount = await dbContext.Events.CountAsync();
        var expectedValues = await dbContext
            .GetEvents()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.ToOutputData())
            .ToListAsync();

        // When
        var pageResult = await paginator.PaginateAsync(
            dbContext.GetEvents(),
            page, pageSize, x => x.ToOutputData(),
            CancellationToken.None);

        // Then
        Assert.Equal(expectedPageCount, pageResult.PageCount);
        Assert.Equal(page, pageResult.PageNumber);
        Assert.Equal(expectedPageSize, pageResult.PageSize);
        Assert.Equal(expectedTotalCount, pageResult.TotalCount);
        Assert.Equal(expectedPageSize, pageResult.Values.Count());
        Assert.Equal(expectedValues, pageResult.Values);
    }
}    
