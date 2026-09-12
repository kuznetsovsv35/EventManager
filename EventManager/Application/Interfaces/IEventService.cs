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
    IEnumerable<EventOutputData> GetAllEvents();

    /// <summary>
    /// Возвращает отфильтрованный набор с разбивкой по страницам.
    /// </summary>
    /// <param name="filterParams">Параметры фильтра.</param>
    /// <param name="pageParams">Параметры разбивки.</param>
    /// <returns></returns>
    Task<PaginateResult<EventOutputData>> GetEvents(FilterParams? filterParams, PageParams pageParams, CancellationToken cancellation);

    /// <summary>
    /// Возвращает отфильтрованный набор.
    /// </summary>
    /// <param name="filterParams">Параметры фильтра.</param>
    /// <returns></returns>
    IAsyncEnumerable<EventOutputData> GetEvents(FilterParams? filterParams);

    /// <summary>
    /// Получить событие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор</param>
    /// <param name="cancellation"></param>
    /// <returns>Экземпляр найденного события или null (если не найдено).</returns>
    Task<EventOutputData> GetEventAsync(Guid id, CancellationToken cancellation);

    /// <summary>
    /// Создает новое событие.
    /// </summary>
    /// <param name="data">Данные о событии.</param>
    /// <returns>СЭкземпляр созданного события.</returns>
    Task<EventOutputData> CreateEventAsync(EventInputData data, CancellationToken cancellation);

    /// <summary>
    /// Обновление данных о событии.
    /// </summary>
    /// <param name="id">Идентификатор обновляемого события.</param>
    /// <param name="data">Данные события.</param>
    /// <returns>Данные о событии после обновления.</returns>
    Task<EventOutputData> UpdateEventAsync(Guid id, EventInputData data, CancellationToken cancellation);

    /// <summary>
    /// Удаляет событие с идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <returns>Информация о удаленном событии.</returns>
    Task<EventOutputData> DeleteEventAsync(Guid id, CancellationToken cancellation);
}
