using System.Linq.Expressions;
using EventManager.Application.Interfaces;
using EventManager.Infrastructure;

namespace EventManager.Application.Services;

public class FilterService<T> : IFilter<T>
{
    public IFilter<T> AddCondition(Expression<Func<T, bool>> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        _expression = null;
        _conditions.Add(predicate);
        return this;
    }

    public IQueryable<T> ApplyFilter(IQueryable<T> values)
    {
        if (Expression is Expression<Func<T, bool>> expression)
            return values.Where(expression);

        return values;
    }

    public IFilter<T> Reset()
    {
        _expression = null;
        _conditions.Clear();
        return this;
    }

    public Expression<Func<T, bool>>? Expression => _expression ??= CreateExpression();

    Expression<Func<T, bool>>? CreateExpression()
        => _conditions.Aggregate(_expression, (a, c) => a == null ? c : a.And(c));

    readonly List<Expression<Func<T, bool>>> _conditions = [];
    Expression<Func<T, bool>>? _expression;
}