using Microsoft.AspNetCore.Mvc;

namespace Common.Middleware.ErrorHandling;

public interface IProblemDetailsMapper
{
    ProblemDetails? MapException(Exception exception);
}
