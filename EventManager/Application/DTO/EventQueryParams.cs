using Microsoft.AspNetCore.Components;

namespace EventManager.Application.DataTransfer;

public class EventQueryParams
{
    public string? Title { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; }
}