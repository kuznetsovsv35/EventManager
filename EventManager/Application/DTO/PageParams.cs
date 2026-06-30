using System.ComponentModel.DataAnnotations;

namespace EventManager.Application.DataTransfer;

public class PageParams
{
    public const int DefaultPageNumber = 1;

    public const int DefaultPageSize = 10;

    public const int MinPageNumber = 1;

    public const int MaxPageNumber = int.MaxValue / 2;

    public const int MinPageSize = 1;

    public const int MaxPageSize = int.MaxValue / 2;

    public const string ErrorMessage = "Значение должно быть положительным.";

    [Range(MinPageNumber, MaxPageNumber, ErrorMessage = PageParams.ErrorMessage)]
    public int CurrentPage { get; init; } = DefaultPageNumber;

    [Range(MinPageSize, MaxPageSize, ErrorMessage = PageParams.ErrorMessage)]
    public int PageSize { get; init; } = DefaultPageSize;

    public static readonly PageParams Default = new()
    {
        CurrentPage = DefaultPageNumber,
        PageSize = DefaultPageSize
    };

    public static readonly PageParams NoPages = new()
    {
        CurrentPage = MinPageNumber,
        PageSize = MaxPageSize
    };
}

public partial class EventQueryParams
{
    public static implicit operator PageParams(EventQueryParams queryParams)
        => new() 
        { 
            CurrentPage =queryParams.Page, 
            PageSize = queryParams.PageSize
        };
}