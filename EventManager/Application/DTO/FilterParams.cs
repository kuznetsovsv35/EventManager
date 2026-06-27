namespace EventManager.Application.DataTransfer;

public record class FilterParams(string? Title, DateTime? From, DateTime? To);

public partial class EventQueryParams
{    
    public static implicit operator FilterParams(EventQueryParams queryParams)
        => new(queryParams.Title, queryParams.From, queryParams.To);
}