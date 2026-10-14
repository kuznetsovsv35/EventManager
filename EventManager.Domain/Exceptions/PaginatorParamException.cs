namespace EventManager.Domain.Exceptions;

/// <summary>
/// Исключение - ошибка валидации параметров разбивки на страницы.
/// </summary>
public class PaginatorParamException : ArgumentException
{
    public int ParamValue { get; }

    public PaginatorParamException(string paramName, int paramValue)
        : this(paramName, paramValue, null) { }

    public PaginatorParamException(string paramName, int paramValue, Exception? innerException)
        : base("Неверное значение параметра разбивки на страницы", paramName, innerException)
    {
        ParamValue = paramValue;
    }
}