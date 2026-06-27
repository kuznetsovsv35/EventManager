using System.Linq.Expressions;
using EventManager.Models;

namespace EventManager.Application.Interfaces;

public interface IEventFilter
{
    /// <summary>
    /// Возвращает выражение фильтра.
    /// </summary>
    Expression<Func<Event, bool>>? Expression { get; }

    /// <summary>
    /// Сброс фильтра.
    /// </summary>
    void Clear();

    /// <summary>
    /// Добавить условие вхождения строки в заголовок.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    IEventFilter TitleContains(string value);

    /// <summary>
    /// Добавить условие начала диапазона дат.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    IEventFilter UseFromDate(DateTime date);

    /// <summary>
    /// Добавить условие конца диапазоны дат.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    IEventFilter UseToDate(DateTime date);
}