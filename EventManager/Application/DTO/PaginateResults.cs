namespace EventManager.Application.DataTransfer;

/// <summary>
/// Структура результата разбивки по страницам.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="TotalCount">Общее количество элементов последовательности.</param>
/// <param name="Values">Последовательность элементов, размещенных на запрашиваемой странице.</param>
/// <param name="PageNumber">Номер запрашиваемой страницы.</param>
/// <param name="PageCount">Количество страниц, требоуемое для вывода всей последовательности.</param>
/// <param name="PageSize">Количество элементов на текущей странице. </param>
public record class PaginateResult<T>(int TotalCount, IEnumerable<T> Values, int PageNumber, int PageCount, int PageSize);