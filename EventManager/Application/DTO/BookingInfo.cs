using EventManager.Models;

namespace EventManager.Application.DataTransfer;

public class BookingInfo
{
    public required Guid Id { get; init; }

    public required Guid EventId { get; init; }

    public required BookingStatus Status { get; init; }

    public required DateTime CreatedAt { get; init; }

    public DateTime? ProcessedAt { get; init; }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj is BookingInfo info)
            return CreatedAt == info.CreatedAt
                && EventId == info.EventId
                && Id == info.Id
                && ProcessedAt ==info.ProcessedAt
                && Status == info.Status;

        return base.Equals(obj);
    }

    public override int GetHashCode()
        => Id.GetHashCode();
}