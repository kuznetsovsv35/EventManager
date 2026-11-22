using Common.Validation;
using EventsService.Domain;

namespace EventsService.Application.DTO;

public static class Extensions
{
    public static Event ToEvent(this EventInputData data)
    {
        data.Check();

        return new Event(data.TotalSeats)
        {
            Title = data.Title!,
            Description = data.Description,
            StartAt = data.StartAt,
            EndAt = data.EndAt
        };
    }

    public static Event Update(this EventInputData data, Event e)
    {
        data.Check();

        e.Title = data.Title!;
        e.Description = data.Description;
        e.StartAt = data.StartAt;
        e.EndAt = data.EndAt;
        e.UpdateTotalSeats(data.TotalSeats);
        return e;
    }

    public static EventOutputData ToOutputData(this Event e)
        => new()
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            StartAt = e.StartAt,
            EndAt = e.EndAt,
            TotalSeats = e.TotalSeats,
            AvailableSeats = e.AvailableSeats,
        };
}
