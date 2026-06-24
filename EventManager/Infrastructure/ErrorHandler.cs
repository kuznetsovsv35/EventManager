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
        => logger.LogError(exception, "Необработанное исключение: Method: {Method}, Path: {Path}", request.Method, request.Path);

    static int MapStatusCodeToException(Exception exception)
        => exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError,
        };

    static async Task MakeErrorResponse(HttpResponse response, Exception exception)
    {
        response.StatusCode = MapStatusCodeToException(exception);

        var respBody = new ProblemDetails
        {
            Status = response.StatusCode, 
            Title = exception.Message,
            Type = $"{exception.GetType().Namespace}.{exception.GetType().Name}",
            Detail = exception.InnerException?.Message
        };

        if (exception.Data.Count > 0)
        {
            foreach(var key in exception.Data.Keys.Cast<object>())
            {
                if (key?.ToString() is string name)
                {
                    respBody.Extensions.Add(KeyValuePair.Create(name, exception.Data[key]));
                }
            }
        }

        response.ContentType = MediaTypeNames.Application.Json;
        await response.WriteAsJsonAsync(respBody);
    }
}