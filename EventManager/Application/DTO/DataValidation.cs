using System.ComponentModel.DataAnnotations;

namespace EventManager.Application.DataTransfer;

/// <summary>
/// Атрибут валидации входных данных.
/// </summary>
public class DataValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return new ValidationResult($"Объект {nameof(EventInputData)} не может быть null.");

        if (value is EventInputData data)
            return Check(data);
        
        return base.IsValid(value, validationContext);
    }

    internal static ValidationResult? Check(EventInputData data)
    {
        if (string.IsNullOrWhiteSpace(data.Title))
            return new ValidationResult("Заголовок события не может быть пустым.", [nameof(data.Title)]);

        if (data.EndAt <= data.StartAt)
            return new ValidationResult($"Окончание не может быть раньше начала события.", [nameof(data.EndAt), nameof(data.StartAt)]);

        return ValidationResult.Success;

    }
}