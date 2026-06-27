namespace EventManager.Application.DataTransfer;

public record class FilterParams(string? Title, DateTime? From, DateTime? To);

public partial class EventQueryParams
{    
    public static implicit operator FilterParams?(EventQueryParams? queryParams)
        => queryParams?.Title is null && queryParams?.From is null && queryParams?.To is null
        ? null : new(queryParams?.Title, queryParams?.From, queryParams?.To);
}