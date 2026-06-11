using EventManager.Application.DataTransfer;

namespace EventManager.Application.Infrastructure;

/// <summary>
/// Интерфейс управления событиями.
/// </summary>
interface IEventService
{
    /// <summary>
    /// Получить все события.
    /// </summary>
    /// <returns>Список событий.</returns>
    IList<EventOutputData> GetAllEvents();

    /// <summary>
    /// Получить событие по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор</param>
    /// <returns>Экземпляр найденного события или null (если не найдено).</returns>
    EventOutputData? GetEvent(Guid id);

    /// <summary>
    /// Создает новое событие.
    /// </summary>
    /// <param name="data">Данные о событии.</param>
    /// <returns>СЭкземпляр созданного события.</returns>
    EventOutputData CreateEvent(EventInputData data);

    /// <summary>
    /// Обновление данных о событии.
    /// </summary>
    /// <param name="id">Идентификатор обновляемого события.</param>
    /// <param name="data">Данные события.</param>
    /// <returns>Данные о событии после обновления.</returns>
    EventOutputData? UpdateEvent(Guid id, EventInputData data);

    /// <summary>
    /// Удаляет событие с идентификатором.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <returns>Информация о удаленном событии.</returns>
    EventOutputData? DeleteEvent(Guid id);
}
