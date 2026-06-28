using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure;

namespace EventManager.Application.Services;

public class PaginateService<T> : IPaginator<T>
{
    public PaginateResult<TView> Paginate<TView>(IQueryable<T> values, int page, int pageSize, Func<T, TView> viewFactory)
    {
        if (page <= 0)
            throw new PaginatorParamException(nameof(page), page);
        
        if (pageSize <= 0)
            throw new PaginatorParamException(nameof(pageSize), pageSize);
        
        return new(
            (int)Math.Ceiling((double)values.Count() / pageSize),
            values.Skip((page - 1) * pageSize).Take(pageSize).ToList().Select(v => viewFactory(v)),
            page, pageSize);
    }
}