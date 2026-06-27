using EventManager.Models;
using EventManager.Application.DataTransfer;

namespace EventManager.Application.Interfaces;

/// <summary>
/// Интерфейс управления событиями.
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Получить все события.
    /// </summary>
    /// <returns>Список событий.</returns>
    PaginateResult GetAllEvents();

    /// <summary>
    /// Возвращает отфильтрованный набор с разбивкой по страницам.
    /// </summary>
    /// <param name="filterParams">Параметры фильтра.</param>
    /// <param name="pageParams">Параметры разбивки.</param>
    /// <returns></returns>
    PaginateResult GetEvents(FilterParams? filterParams, PageParams pageParams);

    /// <summary>
    /// Получить событие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор</param>
    /// <returns>Экземпляр найденного события или null (если не найдено).</returns>
    Event? GetEvent(Guid id);

    /// <summary>
    /// Создает новое событие.
    /// </summary>
    /// <param name="data">Данные о событии.</param>
    /// <returns>СЭкземпляр созданного события.</returns>
    Event CreateEvent(EventInputData data);

    /// <summary>
    /// Обновление данных о событии.
    /// </summary>
    /// <param name="id">Идентификатор обновляемого события.</param>
    /// <param name="data">Данные события.</param>
    /// <returns>Данные о событии после обновления.</returns>
    Event? UpdateEvent(Guid id, EventInputData data);

    /// <summary>
    /// Удаляет событие с идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <returns>Информация о удаленном событии.</returns>
    Event? DeleteEvent(Guid id);
}
