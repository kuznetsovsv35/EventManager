using System.ComponentModel.DataAnnotations;

namespace EventManager.Application.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class UserRequestValidationAttribute : ValidationAttribute
{
    
}