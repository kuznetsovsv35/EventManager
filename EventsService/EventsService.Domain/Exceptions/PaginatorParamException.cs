namespace EventsService.Domain.Exceptions;

/// <summary>
/// Исключение - ошибка валидации параметров разбивки на страницы.
/// </summary>
public class PaginatorParamException(string paramName, int paramValue, Exception? innerException) 
    : ArgumentException("Неверное значение параметра разбивки на страницы", paramName, innerException)
{
    public int ParamValue { get; } = paramValue;

    public PaginatorParamException(string paramName, int paramValue)
        : this(paramName, paramValue, null) { }
}