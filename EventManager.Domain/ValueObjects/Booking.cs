namespace EventManager.Domain.ValueObjects;

/// <summary>
/// Статус брони.
/// </summary>
public enum BookingStatus
{
    /// <summary>
    /// Бронь, ожидающая обработку.
    /// </summary>
    Pending,
    
    /// <summary>
    /// Подтвержденная бронь.
    /// </summary>
    Confirmed,
    
    /// <summary>
    /// Отклоненная бронь.
    /// </summary>
    Rejected,
}

/// <summary>
/// Сущность "Бронь№
/// </summary>
public class Booking
{
    /// <summary>
    /// Уникальный идентификатор.
    /// </summary>
    public Guid Id { get; private set; }
    
    /// <summary>
    /// Идентификатор связанного события.
    /// </summary>
    public Guid EventId { get; private set; }
    
    /// <summary>
    /// Ссылка на связанное события.
    /// </summary>
    public Event Event {get; private set; } = null!;
    
    /// <summary>
    /// Статус бронирования.
    /// </summary>
    public BookingStatus Status { get; private set; }
    
    /// <summary>
    /// Момент создания брони.
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    
    /// <summary>
    /// Момент обработки брони сервисом.
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }
    
    /// <summary>
    /// Приватный конструктор без параметров.
    /// </summary>
    Booking() { }
    
    /// <summary>
    /// Конструктор события.
    /// </summary>
    /// <param name="eventId"></param> 
    public Booking(Guid eventId)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        Status = BookingStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Подтверждает бронь.
    /// </summary>
    /// <returns></returns>
    public bool Confirm() => TryChangeStatus(BookingStatus.Confirmed);
    
    /// <summary>
    /// Отклоняет бронь.
    /// </summary>
    /// <returns></returns>
    public bool Reject() => TryChangeStatus(BookingStatus.Rejected);
    
    /// <summary>
    /// Попытка изменить статус брони.
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    bool TryChangeStatus(BookingStatus status)
    {
        if (Status == BookingStatus.Pending || status == BookingStatus.Rejected)
        {
            Status = status;
            ProcessedAt = DateTime.UtcNow;
            return true;
        }
        return false;
    }
}