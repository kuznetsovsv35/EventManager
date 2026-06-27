namespace EventManager.Application.DataTransfer;

public record class PaginateResult(int TotalCount, IEnumerable<EventOutputData> Values, int PageNumber, int PageSize);