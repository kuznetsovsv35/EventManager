namespace EventManager.Application.DataTransfer;

public class PageParams(int currentPage, int pageSize)
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 10;

    public int CurrentPage { get; } = currentPage;
    public int PageSize { get; } = pageSize;

    public static readonly PageParams Default = new(DefaultPageNumber, DefaultPageSize);

    public static readonly PageParams NoPages = new(1, int.MaxValue);
}

public partial class EventQueryParams
{
    public static implicit operator PageParams(EventQueryParams queryParams)
        => new (queryParams.Page, queryParams.PageSize);
}