using System.ComponentModel.DataAnnotations;
using EventManager.Models;

namespace EventManager.Application.DataTransfer;

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

    public static void Check(this EventInputData data)
    {
        if (EventInputDataValidationAttribute.Check(data).FirstOrDefault() is ValidationResult result)
            throw new ValidationException(result, null, data);
    }

    public static BookingInfo ToInfo(this Booking booking)
        => new()
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt,
        };
}