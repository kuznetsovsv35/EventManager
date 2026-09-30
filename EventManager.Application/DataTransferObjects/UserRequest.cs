using System.ComponentModel.DataAnnotations;
using EventManager.Application.Validators;

namespace EventManager.Application.DataTransferObjects;

/// <summary>
/// Запрос на действия с сущностью Пользователь.
/// </summary>
[UserRequestValidation]
public class UserRequest : ValidatingObject
{
    [UserRequestValidation("Неверное имя входа"), MinLength(1)]
    public string Login { get; set; } = string.Empty;

    public string? Password { get; set; }
}