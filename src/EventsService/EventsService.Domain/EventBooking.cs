namespace EventsService.Domain;

/// <summary>
/// Элемент журнала бронирований.
/// </summary>
public class EventBooking
{
    EventBooking() {}
    /// <summary>
    /// Идентификатор бронирования
    /// </summary>
    public Guid BookingId { get; private set; }
    /// <summary>
    /// Идентификатор события
    /// </summary>
    public Guid EventId { get; private set; }
    /// <summary>
    /// Ссылка на событие.
    /// </summary>
    public Event? Event { get; private set; } 
    /// <summary>
    /// Количество резервированных мест.
    /// </summary>
    public int ReservedSeats { get; private set; } = 1;
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public Guid? UserId {get; set; }
    /// <summary>
    /// Момент времени создания (для дальнейшего)
    /// </summary>
    public DateTime? RequestedAt { get; private set; }
    /// <summary>
    /// Момент времени подтверждения.
    /// </summary>
    public DateTime? ConfirmedAt { get; private set; }
    /// <summary>
    /// Момент времени отмены.
    /// </summary>
    public DateTime? CancelledAt { get; private set; }
    /// <summary>
    /// Конструктор нового элемента журнала.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="bookingId"></param>
    public EventBooking(Event @event, Guid bookingId)
    {
        BookingId = bookingId;
        EventId = @event.Id;
        Event = @event;
    }
    public EventBooking(Guid eventId, Guid bookingId)
    {
        BookingId = bookingId;
        EventId = eventId;
    }
    /// <summary>
    /// События существует.
    /// </summary>
    public bool IsEventValid => Event is not null;
    /// <summary>
    /// Элемент журнала обработан.
    /// </summary>
    public bool IsProcessed => IsConfirmed || IsCancelled;
    /// <summary>
    /// Бронирование подтверждено (места зарезервированы)
    /// </summary>
    public bool IsConfirmed => ConfirmedAt.HasValue;
    /// <summary>
    /// Бронирование отклонено/отменено.
    /// </summary>
    public bool IsCancelled => CancelledAt.HasValue;
    /// <summary>
    /// Попытка подтвердить бронирование (зарезервировать места).
    /// </summary>
    /// <returns>null - состояние не изменилось, true - места зарезервированы, false - нельзя зарезервировать</returns>
    public bool? Confirm(int reservedSeats)
    {
        if (IsProcessed)
            return null;

        ReservedSeats = reservedSeats;
        if (Event?.TryReserveSeats(ReservedSeats) == true)
        {
            ConfirmedAt = DateTime.UtcNow;
            return true;
        }

        CancelledAt = DateTime.UtcNow;
        return false;
    }
    /// <summary>
    /// Попытка отменить бронирование
    /// </summary>
    /// <returns>null - состояние не изменилось, true - места возвращены, false - пометка об отменен. </returns>
    public bool? Cancel()
    {
        if (IsCancelled)
            return null;

        if (IsConfirmed)
        {
            Event?.ReleaseSeats(ReservedSeats);
            CancelledAt = DateTime.UtcNow;
            return true;
        }
        
        CancelledAt = DateTime.UtcNow;
        return false;
    }
}
