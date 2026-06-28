using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;

namespace EventManager.Application.Services;

public class PaginateService<T> : IPaginator<T>
{
    public PaginateResult<TView> Paginate<TView>(IQueryable<T> values, int page, int pageSize, Func<T, TView> viewFactory)
    {
        return new(
            (int)Math.Ceiling((double)values.Count() / pageSize),
            values.Skip((page - 1) * pageSize).Take(pageSize).ToList().Select(v => viewFactory(v)),
            page, pageSize);
    }
}