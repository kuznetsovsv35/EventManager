using System.ComponentModel.DataAnnotations;
using Authorization.Exceptions;
using Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Common.Middleware.ErrorHandling;

public class ProblemDetailsMapper<T> : IProblemDetailsMapper where T : class, IProblemDetailsMapper, new()
{
    readonly IProblemDetailsMapper _customMapper = new T();

    public ProblemDetails? MapException(Exception exception)
    {
        if (_customMapper.MapException(exception) is ProblemDetails details)
            return details;
        
        return DefaultMap(exception);
    }

    ProblemDetails DefaultMap(Exception exception)
        => exception switch
        {
            ValidationException ve => ProblemDetailsFactory.ValidationProblem(ve.ValidationResult, ve.Message),
            ForbiddenException forbidden => ProblemDetailsFactory.Forbid(forbidden.Message),
            ObjectNotFoundException<Guid> notFound => ProblemDetailsFactory.NotFound($"{notFound.Message}: (ID={notFound.ObjectKey})."),
            _ => ProblemDetailsFactory.InternalServiceError(exception.Message)        
        };
}
