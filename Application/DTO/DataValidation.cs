using System.ComponentModel.DataAnnotations;

namespace EventManager.Application.DataTransfer;

/// <summary>
/// Атрибут валидации входных данных.
/// </summary>
public class DataValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is EventInputData data)
        {
            if (string.IsNullOrWhiteSpace(data.Title))
                return new ValidationResult($"Свойство \"{nameof(data.Title)}\" не может быть пустым.");

            if (data.EndAt <= data.StartAt)
                return new ValidationResult("Окончание не может быть раньше начала события.");

            return ValidationResult.Success;
        }
        return base.IsValid(value, validationContext);
    }
}