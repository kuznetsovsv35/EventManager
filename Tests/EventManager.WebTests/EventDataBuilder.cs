using EventManager.Application.DataTransferObjects;

namespace EventManager.WebTests;

internal class EventDataBuilder
{
    public EventDataBuilder UseTitle(string title)
    {
        _title = title;
        return this;
    }

    public EventDataBuilder UseDescription(string? description)
    {
        _description = description;
        return this;
    }

    public EventDataBuilder StartAt(DateTime startAt)
    {
        _startAt = startAt;
        return this;
    }

    public EventDataBuilder HasDuration(TimeSpan duration)
    {
        _duration = TimeSpan.FromTicks(Math.Clamp(duration.Ticks, TimeSpan.FromMinutes(1).Ticks, TimeSpan.MaxValue.Ticks));
        return this;
    }

    public EventDataBuilder HasTotalSeats(int seats)
    {
        _totalSeats = seats;
        return this;
    }

    public EventInputData Build()
        => new()
        {
            Title = _title ?? $"Event {Guid.NewGuid()}",
            Description = _description,
            EndAt = _startAt + _duration,
            StartAt = _startAt,
            TotalSeats = Math.Max(_totalSeats, 1),
        };

    string? _title;
    string? _description;
    DateTime _startAt = DateTime.UtcNow;
    TimeSpan _duration = TimeSpan.FromHours(1);
    int _totalSeats;
}