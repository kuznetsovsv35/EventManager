using Microsoft.AspNetCore.Mvc;

namespace Common.Middleware.ErrorHandling;

public class ProblemDetailsBuilder
{
    public ProblemDetails Details { get; }

    ProblemDetailsBuilder(ProblemDetails details) => Details = details;

    public static ProblemDetailsBuilder Create<T>(int status, Action<T>? builder = null) where T: ProblemDetails, new()
    {
        var details = new T() { Status = status };
        ProblemDetailsBuilder b = new(details);
        builder?.Invoke(details);
        return b;
    }

    public static ProblemDetailsBuilder Create(int status) 
        => Create<ProblemDetails>(status);

    public ProblemDetailsBuilder AddInstance(string instance)
    {
        Details.Instance = instance;
        return this;
    }

    public ProblemDetailsBuilder UseType(string type)
    {
        Details.Type = type;
        return this;
    }

    public ProblemDetailsBuilder UseTitle(string title)
    {
        Details.Title = title;
        return this;
    }

    public ProblemDetailsBuilder UseDetail(string detail)
    {
        Details.Detail = detail;
        return this;
    }
}
