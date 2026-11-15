using System.ComponentModel.DataAnnotations;
using Common.Middleware.ErrorHandling;
using EventsService.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EventsService.Middleware;

public class EventsProblemDetailsMapper : IProblemDetailsMapper
{
    public ProblemDetails? MapException(Exception exception)
        => exception switch
        {
            PaginatorParamException pe => ProblemDetailsFactory.ValidationProblem
            (
                new ValidationResult(pe.Message, (pe.ParamName is string pn) ? new[] { pn } : null),
                    pe.Message, $"параметр: {pe.ParamName}, значение: {pe.ParamValue}"
            ),

            _ => null
        };
}
