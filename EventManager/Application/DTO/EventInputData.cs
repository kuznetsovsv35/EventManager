using System.ComponentModel.DataAnnotations;
using EventManager.Models;

namespace EventManager.Application.DataTransfer;

/// <summary>
/// Водные данные запроса создания и обновления события.
/// </summary>
[EventInputDataValidation]
public class EventInputData
{
    [EventInputDataValidation("Загаловок события не может быть пустым.")]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [EventInputDataValidation("Момент начала события не может быть позже момента окончания.")]
    public DateTime StartAt { get; set; }

    [EventInputDataValidation("Момент окончания события не может быть раньше момента начала.")]
    public DateTime EndAt { get; set; }

    public EventInputData() { }

    internal EventInputData(Event source)
    {
        Title = source.Title;
        Description = source.Description;
        StartAt = source.StartAt;
        EndAt = source.EndAt;
    }

    void Check()
    {
        if (EventInputDataValidationAttribute.Check(this) is ValidationResult result)
            throw new ValidationException(result, null, this);
    }

    internal Event ToEvent()
    {
        Check();

        return new Event()
        {
            Id = Guid.NewGuid(),
            Title = this.Title!,
            Description = this.Description,
            StartAt = this.StartAt,
            EndAt = this.EndAt
        };
    }

    internal void Update(Event dest)
    {
        Check();

        dest.Title = Title!;
        dest.Description = Description;
        dest.StartAt = StartAt;
        dest.EndAt = EndAt;
    }
}
