namespace EventManager.Models;

/// <summary>
/// Модель данных события.
/// </summary>
public class Event
{
    public required Guid Id { get; set; }

    public required string Title { get; set; }

    public string? Description { get; set; }

    public required DateTime StartAt { get; set; }

    public required DateTime EndAt { get; set; }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj is Event other)
            return Id == other.Id
                && Title == other.Title
                && Description == other.Description
                && StartAt == other.StartAt
                && EndAt == other.EndAt;

        return base.Equals(obj);
    }

    public override int GetHashCode() => Id.GetHashCode();
}