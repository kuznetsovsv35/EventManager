namespace BookingService.Domain;

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

    /// <summary>
    /// Отменена пользователем.
    /// </summary>
    Cancelled,
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
    /// Пользователь, создавший бронь.
    /// </summary>
    public Guid UserId { get; private set; }

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
    public Booking(Guid eventId, Guid userId)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        UserId = userId;
        Status = BookingStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Подтверждает бронь.
    /// </summary>
    /// <returns></returns>
    public bool Confirm()
    { 
        if (Status == BookingStatus.Pending)
        {
            ChangeStatus(BookingStatus.Confirmed);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Отклоняет бронь.
    /// </summary>
    /// <returns></returns>
    public bool Reject() 
    { 
        if (Status == BookingStatus.Pending)
        {
            ChangeStatus(BookingStatus.Rejected);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Отменяет бронь.
    /// </summary>
    /// <returns></returns>
    public bool Cancel() 
    {
        if (this is { Status: BookingStatus.Pending or BookingStatus.Confirmed } ) 
        {
            ChangeStatus(BookingStatus.Cancelled);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Попытка изменить статус брони.
    /// </summary>
    /// <param name="status"></param>
    void ChangeStatus(BookingStatus status)
    {
        if (Status == BookingStatus.Pending)
            ProcessedAt = DateTime.UtcNow;

        Status = status;
    }
}