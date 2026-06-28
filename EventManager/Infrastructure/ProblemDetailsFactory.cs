using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.Infrastructure;

public class ProblemDetailsBuilder<T>(T problem) where T : ProblemDetails
{
    public T Problem { get; } = problem;

    public ProblemDetailsBuilder<T> Build(Action<T> build)
    {
        build?.Invoke(Problem);
        return this;
    }

    public ProblemDetailsBuilder<T> AddInstance(string instance)
    {
        Problem.Instance = instance;
        return this;
    }
}

public static class ProblemDetailsFactory
{
    public static ProblemDetailsBuilder<ProblemDetails> InternalServiceError(string? detail = null, string? title = null)
        => new(new()
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Title = title ?? "Внутренняя ошибка сервера",
            Detail = detail ?? "Произошла непредвиденная ошибка. Попробуйте повторить запрос позже.",
            Status = StatusCodes.Status500InternalServerError,
        });

    public static ProblemDetailsBuilder<ProblemDetails> ValidationProblem(ValidationResult result, string message, string? detail = null, string? title = null)
    {
        var details = new ValidationProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            Title = title ?? "Ошибка валидации запроса",
            Detail = detail ?? "Один или несколько параметров содержат недопустимые значения.",
            Status = StatusCodes.Status400BadRequest,
        };

        foreach (var member in result.MemberNames)
        {
            details.Errors[member] = [result.ErrorMessage ?? message];
        }

        return new(details);
    }

    public static ProblemDetailsBuilder<ProblemDetails> NotFound(string? detail = null, string? title = null)
        => new(new()
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
            Title = title ?? "Ресурс не найден.",
            Detail = detail ?? "Ресурс не найден.",
            Status = StatusCodes.Status404NotFound,
        });
}