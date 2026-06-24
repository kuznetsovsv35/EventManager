using System.ComponentModel.DataAnnotations;
using EventManager.Models;

namespace EventManager.Application.DataTransfer;

/// <summary>
/// Водные данные запроса создания и обновления события.
/// </summary>
[DataValidation]
public class EventInputData
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public EventInputData() { }

    internal EventInputData(Event source)
    {
        Title = source.Title;
        Description = source.Description;
        StartAt = source.StartAt;
        EndAt = source.EndAt;
    }

    static readonly ValidationAttribute _validationAttribute = new DataValidationAttribute();
    void Check()
    {
        var result = DataValidationAttribute.Check(this);
        if (result != ValidationResult.Success && result?.ErrorMessage is not null)
            throw new ValidationException(result, _validationAttribute, this);
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
