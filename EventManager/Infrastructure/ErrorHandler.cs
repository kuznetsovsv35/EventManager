using System.Net.Mime;
using HttpContent = Microsoft.AspNetCore.Http.HttpContext;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.Infrastructure;

public class ErrorHandler(RequestDelegate next, ILogger<ErrorHandler> logger)
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
            await MakeErrorResponse(content.Response, ex);
        }
    }

    void LogException(HttpRequest request, Exception exception)
        => logger.LogError(exception, $"Unhandled exception, Method: {request.Method}, Path: {request.Path}");

    async Task<int> MapStatusCodeToException(Exception exception)
        => exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError,
        };

    async Task MakeErrorResponse(HttpResponse response, Exception exception)
    {
        response.StatusCode = await MapStatusCodeToException(exception);

        var respBody = new ProblemDetails
        {
            Status = response.StatusCode, 
            Detail = exception.Message
        };
        response.ContentType = MediaTypeNames.Application.Json;
        await response.WriteAsJsonAsync(respBody);
    }
}