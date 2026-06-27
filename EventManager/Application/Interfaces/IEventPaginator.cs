using EventManager.Application.DataTransfer;
using EventManager.Models;

namespace EventManager.Application.Interfaces;

public interface IEventPaginator
{
    /// <summary>
    /// Возвращает результат отбора с разбивкой по страницам.
    /// </summary>
    /// <param name="events">Объект запроса.</param>
    /// <param name="pageParams">Параметры разбивки по страницам.</param>
    /// <returns></returns>
    PaginateResult Paginate(IQueryable<Event> events, PageParams pageParams);    
}