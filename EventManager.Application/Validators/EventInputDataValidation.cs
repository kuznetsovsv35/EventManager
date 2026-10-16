using System.ComponentModel.DataAnnotations;
using EventManager.Application.DataTransferObjects;

namespace EventManager.Application.Validators;

/// <summary>
/// Атрибут валидации входных данных события.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class EventInputDataValidationAttribute : ValidationBaseAttribute
{
    public EventInputDataValidationAttribute(string errorMessage) : base(errorMessage) { }

    public EventInputDataValidationAttribute() : base(typeof(EventInputData)) { }

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
                case nameof(data.TotalSeats):
                    if (value is int totalSeats && totalSeats <= 0)
                        return CreateResult(context);
                    break;
            }

            return ValidationResult.Success;
        }

        return CreateResult(context);
    }
}