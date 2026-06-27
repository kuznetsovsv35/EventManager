namespace EventManager.Application.DataTransfer;

public record class PageParams(int CurrentPage, int PageSize);

public partial class EventQueryParams
{
    public static implicit operator PageParams(EventQueryParams queryParams)
        => new (queryParams.Page, queryParams.PageSize);
}