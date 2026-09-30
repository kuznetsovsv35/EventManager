using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Application.DataTransferObjects;

public static class Validation
{
    public static void Check<T>(this T obj) where T : ValidatingObject
    {
        if (obj.Validate().FirstOrDefault() is ValidationResult result)
            throw new ValidationException(result, null, obj);
        
    }

    public static IReadOnlyCollection<ValidationResult> Validate<T>(this T obj) where T : ValidatingObject
    {
        var results = new Collection<ValidationResult>();
        var isValid = Validator.TryValidateObject(obj, new ValidationContext(obj), results, true);

        if (isValid)
            return [];

        return results;
    }    
}