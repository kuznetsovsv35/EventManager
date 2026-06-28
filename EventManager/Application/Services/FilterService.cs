using System.Linq.Expressions;
using System.Reflection;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure;

namespace EventManager.Application.Services;

public class FilterService<T> : IFilter<T>
{
    public IFilter<T> AddCondition(Expression<Func<T, bool>> predicate)
    {
        _expression = null;
        _conditions.Add(predicate);
        return this;
    }

    public IQueryable<T> ApplyFilter(IQueryable<T> values)
    {
        if (_expression is null)
            _expression = _conditions.Aggregate(_expression, (a, c) => a == null ? c : a.And(c) );
        
        if (_expression is not null)
        {
            return values.Where(_expression);
        }
        else
        {
            if (_conditions.Count == 0)
                return values;
        }

        throw new InvalidFilterCriteriaException();
    }

    public IFilter<T> Reset()
    {
        _expression = null;
        _conditions.Clear();
        return this;
    }

    readonly List<Expression<Func<T, bool>>> _conditions = [];
    Expression<Func<T, bool>>? _expression;
}