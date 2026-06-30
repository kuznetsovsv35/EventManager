namespace EventManager.Application.DataTransfer;

public class FilterParams
{
    public string? Title { get; init; }

    public DateTime? From { get; init; }

    public DateTime? To { get; init; }
}

public partial class EventQueryParams
{    
    public static implicit operator FilterParams?(EventQueryParams? queryParams)
        => queryParams?.Title is null && queryParams?.From is null && queryParams?.To is null
        ? null : new()
        {
            Title = queryParams?.Title, 
            From = queryParams?.From, 
            To = queryParams?.To
        };
}