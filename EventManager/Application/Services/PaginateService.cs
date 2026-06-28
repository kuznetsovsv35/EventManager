using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;

namespace EventManager.Application.Services;

public class PaginateService<T> : IPaginator<T>
{
    public PaginateResult<TView> Paginate<TView>(IQueryable<T> values, int page, int pageSize, Func<T, TView> viewFactory)
    {
        throw new NotImplementedException();
    }
}