using EventManager.Application.Authorization;
using EventManager.Domain.ValueObjects;

namespace EventManager.Application.DataTransferObjects;

public static class DataTransferExtension
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

    public static BookingInfo ToInfo(this Booking booking)
        => new()
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt,
        };

    public static User FromRequest(this RegisterUserRequest request)
        => new(request.Login)
        {
            Role = request.Role,
            Password = request.Password,
        };

    public static UserInfo ToInfo(this User user)
        => new()
        {
            Id = user.Id,
            Login = user.Login,
            Role = user.Role,
        };

    public static UserInfo ToInfo(this ICurrentUser user)
        => new()
        {
            Id = user.Id,
            Login = user.Login,
            Role = user.Role,
        };
}