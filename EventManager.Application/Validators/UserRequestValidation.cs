using System.ComponentModel.DataAnnotations;
using EventManager.Application.DataTransferObjects;

namespace EventManager.Application.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class UserRequestValidationAttribute : ValidationBaseAttribute
{
    public UserRequestValidationAttribute(string errorMessage) : base(errorMessage) { }

    public UserRequestValidationAttribute() : base(typeof(UserRequest)) { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is UserRequest userRequest)
        {
            switch (validationContext.MemberName)
            {
                case nameof(UserRequest.Login):
                    if (string.IsNullOrWhiteSpace(userRequest.Login))
                        return CreateResult(validationContext);
                    break;
            }

            return ValidationResult.Success;
        }

        return CreateResult(validationContext);
    }
}