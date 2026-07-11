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
    public required Guid Id { get; set; }

    /// <summary>
    /// Идентификатор связанного события.
    /// </summary>
    public required Guid EventId { get; set; }

    /// <summary>
    /// Статус бронирования.
    /// </summary>
    public required BookingStatus Status { get; set; }

    /// <summary>
    /// Момент создания брони.
    /// </summary>
    public required DateTime CreatedAt { get; set; }

    /// <summary>
    /// Момент обработки брони сервисом.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    #region  Инфраструктура сравнения брони.
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj is Booking other)
            return Id == other.Id
                && EventId == other.EventId
                && Status == other.Status
                && CreatedAt == other.CreatedAt
                && ProcessedAt == other.ProcessedAt;

        return base.Equals(obj);
    }

    public override int GetHashCode() => Id.GetHashCode();
    #endregion
}