using System.ComponentModel.DataAnnotations;

namespace EventManager.Application.Validators;

/// <summary>
/// Базовый класс атрибута валидации входных данных.
/// </summary>
public abstract class ValidationBaseAttribute : ValidationAttribute
{
    protected ValidationBaseAttribute(string errorMessage) : base(errorMessage) { }
    protected ValidationBaseAttribute(Type type) : this($"Ошибка валидации объекта {type.Name}.") { }
    protected ValidationResult CreateResult(ValidationContext? context)
        => new(ErrorMessage, context?.MemberName is string memberName ? [memberName] : null);
}
