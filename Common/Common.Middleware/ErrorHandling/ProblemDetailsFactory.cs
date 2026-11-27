using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Common.Middleware.ErrorHandling;

public static class ProblemDetailsFactory
{
    public static ProblemDetails InternalServiceError(string? detail = null, string? title = null)
        => ProblemDetailsBuilder.Create(StatusCodes.Status500InternalServerError)
        .UseType("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1")
        .UseTitle(title ?? "Внутренняя ошибка сервера")
        .UseDetail(detail ?? "Произошла непредвиденная ошибка. Попробуйте повторить запрос позже.")
        .Details;

    public static ProblemDetails ValidationProblem(ValidationResult result, string message, string? detail = null, string? title = null)
        => ProblemDetailsBuilder.Create<ValidationProblemDetails>(StatusCodes.Status400BadRequest, d =>
        {
            foreach (var member in result.MemberNames)
                d.Errors[member] = [result.ErrorMessage ?? message];
        })
        .UseType("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1")
        .UseTitle(title ?? "Ошибка валидации запроса")
        .UseDetail(detail ?? "Один или несколько параметров содержат недопустимые значения.")
        .Details;

    public static ProblemDetails BadRequest(string? detail = null, string? title = null)
        => ProblemDetailsBuilder.Create(StatusCodes.Status400BadRequest)
        .UseType("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1")
        .UseTitle(title ?? "Ошибка валидации запроса")
        .UseDetail(detail ?? "Один или несколько параметров содержат недопустимые значения.")
        .Details;

    public static ProblemDetails NotFound(string? detail = null, string? title = null)
        => ProblemDetailsBuilder.Create(StatusCodes.Status404NotFound)
        .UseType("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4")
        .UseTitle(title ?? "Ресурс не найден.")
        .UseDetail(detail ?? "Ресурс не найден.")
        .Details;

    public static ProblemDetails Conflict(string? detail = null, string? title = null)
        => ProblemDetailsBuilder.Create(StatusCodes.Status409Conflict)
        .UseType("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8")
        .UseTitle(title ?? "Конфликт")
        .UseDetail(detail ?? "Состояние ресурса изменено, обновление невозможно")
        .Details;

    public static ProblemDetails Forbid(string? detail = null, string? title = null)
        => ProblemDetailsBuilder.Create(StatusCodes.Status403Forbidden)
        .UseType("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3")
        .UseTitle(title ?? "Доступ запрещен")
        .UseDetail(detail ?? "Нет прав на выполнения операции")
        .Details;

    public static ProblemDetails Unauthorized(string? detail = null, string? title = null)
        => ProblemDetailsBuilder.Create(StatusCodes.Status401Unauthorized)
        .UseType("https://datatracker.ietf.org/doc/html/rfc7235#section-3.1")
        .UseTitle(title ?? "Ошибка аутентификации пользователя")
        .UseDetail(detail ?? "Логин или пароль указан неверно")
        .Details;

}