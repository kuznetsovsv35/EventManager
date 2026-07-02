namespace EventManager.Models;

public enum BookingStatus
{
    Pending,

    Confirmed,

    Rekected,
}

public class Booking
{
    public required Guid Id { get; set; }

    public required Guid EventId { get; set; }

    public required BookingStatus Status { get; set; } = BookingStatus.Pending;

    public required DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? ProcessedAt { get; set; }

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
}