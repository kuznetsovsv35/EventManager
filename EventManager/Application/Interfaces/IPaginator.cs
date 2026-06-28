using EventManager.Application.DataTransfer;

namespace EventManager.Application.Interfaces;

public interface IPaginator<T>
{
    /// <summary>
    /// Возвращает результат отбора с разбивкой по страницам.
    /// </summary>
    /// <param name="values">Объект запроса.</param>
    /// <param name="page">Текущая страница.</param>
    /// <param name="pageSize">Текущая страница.</param>
    /// <param name="viewFactory">Фабрика отображения.</param>
    /// <returns></returns>
    PaginateResult<TView> Paginate<TView>(IQueryable<T> values, int page, int pageSize, Func<T, TView> viewFactory);
}