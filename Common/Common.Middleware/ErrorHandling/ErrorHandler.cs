using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HttpContent = Microsoft.AspNetCore.Http.HttpContext;

namespace Common.Middleware.ErrorHandling;

public class ErrorHandler(RequestDelegate next, IProblemDetailsMapper detailsMapper, ILogger<ErrorHandler> logger)
{
    public async Task Invoke(HttpContent content)
    {
        try
        {
            await next(content);
        }
        catch (Exception ex)
        {
            LogException(content.Request, ex);

            if (content.Response.HasStarted)
            {
                return;
            }

            content.Response.Clear();
            await CreateErrorResponse(content, ex);
        }
    }

    void LogException(HttpRequest request, Exception exception)
        => logger.LogError(exception, "Необработанное исключение: Method: {Method}, Path: {Path}", request.Method, request.Path);

    async Task CreateErrorResponse(HttpContent context, Exception exception)
    {
        var request = context.Request;
        var response = context.Response;

        var details = detailsMapper.MapException(exception)
            ?? ProblemDetailsFactory.InternalServiceError(exception.Message);

        details.Instance = request.Path;
        response.StatusCode = details.Status ?? StatusCodes.Status500InternalServerError;

        response.ContentType = MediaTypeNames.Application.ProblemJson;
        await response.WriteAsJsonAsync(details);
    }
}