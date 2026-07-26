using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Models;

public enum BookingStatus
{
    Pending,

    Confirmed,

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
    /// Статус бронирования.
    /// </summary>
    public BookingStatus Status { get; private set ; }

    /// <summary>
    /// Момент создания брони.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Момент обработки брони сервисом.
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    Booking() {}

    public Booking(Guid eventId)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        Status = BookingStatus.Pending;
        CreatedAt = DateTime.Now;
    }

    public bool Confirm() => TryChangeStatus(BookingStatus.Confirmed);

    public bool Reject() => TryChangeStatus(BookingStatus.Rejected);

    bool TryChangeStatus(BookingStatus status)
    {
        if (Status == BookingStatus.Pending)
        {
            Status = status;
            ProcessedAt = DateTime.Now;
            return true;
        }
        return false;
    }
}