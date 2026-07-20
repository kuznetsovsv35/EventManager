using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Application.Services;

public class PaginateService<T> : IPaginator<T>
{
    PaginateResult<TView> IPaginator<T>.Paginate<TView>(IQueryable<T> values, int page, int pageSize, Func<T, TView> viewFactory)
    {
        if (page <= 0)
            throw new PaginatorParamException(nameof(page), page);

        if (pageSize <= 0)
            throw new PaginatorParamException(nameof(pageSize), pageSize);

        var totalCount = values.Count();
        var pageCount = (int)Math.Ceiling((double)totalCount / pageSize);
        var pageValues = values.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new(
            totalCount,
            pageValues.Select(v => viewFactory(v)),
            page,
            pageCount,
            pageValues.Count);
    }

    async Task<PaginateResult<TView>> IPaginator<T>.PaginateAsync<TView>(
        IQueryable<T> values, 
        int page, int pageSize, Func<T, TView> viewFactory,
        CancellationToken cancellation)
    {
        if (page <= 0)
            throw new PaginatorParamException(nameof(page), page);

        if (pageSize <= 0)
            throw new PaginatorParamException(nameof(pageSize), pageSize);

        var totalCount = await values.CountAsync(cancellation);
        var pageCount = (int)Math.Ceiling((double)totalCount / pageSize);
        var pageValues = await values.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellation);

        return new(
            totalCount,
            pageValues.Select(v => viewFactory(v)),
            page,
            pageCount,
            pageValues.Count);
    }
}