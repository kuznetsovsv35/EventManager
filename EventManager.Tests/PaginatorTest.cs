using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Tests;

/// <summary>
/// Тест разбивки на страницы.
/// </summary>
/// <param name="fixture"></param>
public class PaginatorTest(EventManagerTestContext context) : TestObjectBase, IClassFixture<EventManagerTestContext>
{
    /// <summary>
    /// Тест валидации параметров на страницы.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    [Trait(Category, Category_Paginator)]
    [Theory]
    [InlineData([-1, 10])]
    [InlineData([0, 10])]
    [InlineData([1, -20])]
    [InlineData([1, 0])]
    public async Task ValidateParameters_Fail(int page, int pageSize)
    {
        // Given
        await using var scope = context.CreateAsyncScope();
        var paginator = scope.ServiceProvider.GetRequiredService<IPaginator<Event>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        // When

        // Then
        await Assert.ThrowsAnyAsync<PaginatorParamException>(async ()
            => await paginator.PaginateAsync(dbContext.GetEvents(), page, pageSize, x => x, CancellationToken.None));
    }

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
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var paginator = scope.ServiceProvider.GetRequiredService<IPaginator<Event>>();

        var expectedTotalCount = await dbContext.GetEvents().CountAsync();
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