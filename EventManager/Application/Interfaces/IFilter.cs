using System.Linq.Expressions;

namespace EventManager.Application.Interfaces;

public interface IFilter<T>
{
    /// <summary>
    /// Сброс фильтра.
    /// </summary>
    IFilter<T> Reset();

    /// <summary>
    /// Добавить условие.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    IFilter<T> AddCondition(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Возвращает итоговое выражение.
    /// </summary>
    Expression<Func<T, bool>>? Expression { get; }

    /// <summary>
    /// Применить фильтр к данным.
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    IQueryable<T> ApplyFilter(IQueryable<T> values);
}