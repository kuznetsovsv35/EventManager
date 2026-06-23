using System.ComponentModel.DataAnnotations;
using EventManager.Models;

namespace EventManager.Application.DataTransfer;

/// <summary>
/// Водные данные запроса создания и обновления события.
/// </summary>
[DataValidation]
public class EventInputData
{
    [Required(AllowEmptyStrings = false, ErrorMessage = $"Свойство \"{nameof(Title)}\" не может быть пустым.")]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [Required(ErrorMessage = $"Свойство \"{nameof(StartAt)}\" не может быть пустым.")]
    public DateTime StartAt { get; set; }

    [Required(ErrorMessage = $"Свойство \"{nameof(EndAt)}\" не может быть пустым.")]
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
        if (string.IsNullOrWhiteSpace(Title))
            throw new ValidationException($"Свойство \"{nameof(Title)}\" не может быть пустым");

        if (EndAt <= StartAt)
            throw new ValidationException($"Окончание не может быть раньше начала события (свойства \"{nameof(EndAt)}\" и \"{nameof(StartAt)}\")");
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
