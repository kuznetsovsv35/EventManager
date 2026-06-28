using EventManager.Application.Interfaces;

namespace EventManager.Application.Services;

public class FilterService<T> : IFilter<T>
{
    public IFilter<T> AddCondition(Func<T, bool> predicate)
    {
        throw new NotImplementedException();
    }

    public IQueryable<T> ApplyFilter(IQueryable<T> values)
    {
        throw new NotImplementedException();
    }

    public IFilter<T> Reset()
    {
        throw new NotImplementedException();
    }
}