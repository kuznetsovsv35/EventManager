using EventManager.Models;

namespace EventManager.Application.Interfaces;

/// <summary>
/// Интерфейс доступа к данным.
/// </summary>
public interface IAppDbContext
{
    /// <summary>
    /// Вовзращает queryable объект набора данных.
    /// </summary>
    IQueryable<Event> Events { get; }
    
    /// <summary>
    /// Добавляет событие в набор данных.
    /// </summary>
    /// <param name="event"></param>
    void Add(Event @event);
    
    /// <summary>
    /// Обновляет событие.
    /// </summary>
    /// <param name="event"></param>
    void Update(Event @event);
    
    /// <summary>
    /// Удаляет событие из набора данных.
    /// </summary>
    /// <param name="event"></param>
    void Delete(Event @event);
}