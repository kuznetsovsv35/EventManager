using System.ComponentModel.DataAnnotations;

namespace EventManager.Application.DataTransfer;

/// <summary>
/// Атрибут валидации входных данных.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class EventInputDataValidationAttribute : ValidationAttribute
{
    public EventInputDataValidationAttribute(string errorMessage) : base(errorMessage) {}
    

    public EventInputDataValidationAttribute() : this($"Ошибка валидации объекта {nameof(EventInputData)}.") {}

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        // Валидация типа и ссылки
        if (context?.ObjectInstance is EventInputData data)
        {
            switch (context.MemberName)
            {                
                case nameof(data.Title): 
                    if (string.IsNullOrWhiteSpace(value as string))
                        return CreateResult(context);
                    break;
                case nameof(data.StartAt):
                    if (value is DateTime startAt && data.EndAt <= startAt)
                        return CreateResult(context);
                    break;
                case nameof(data.EndAt):
                    if (value is DateTime endAt && endAt <= data.StartAt)
                        return CreateResult(context);
                    break;
            }
            
            return ValidationResult.Success;
        }

        return CreateResult(context);
    }

    ValidationResult CreateResult(ValidationContext? context)
        => new(ErrorMessage, context?.MemberName is string memnerName ? [memnerName] : null);
    
    internal static  ValidationResult? Check(EventInputData data)
    {
        var results = new List<ValidationResult>();
        
        // For validation tests
        // data.Title = string.Empty;
        // data.StartAt = data.EndAt;
        var isValid = Validator.TryValidateObject(data, new ValidationContext(data), results, true);
        
        if (isValid == false)
        {
            if (results.FirstOrDefault() is ValidationResult result)
                return result;
        }
        
        return ValidationResult.Success;
    }
}