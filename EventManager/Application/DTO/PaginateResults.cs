namespace EventManager.Application.DataTransfer;

public record class PaginateResult<T>(int TotalCount, IEnumerable<T> Values, int PageNumber, int PageSize);