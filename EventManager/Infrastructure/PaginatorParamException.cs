namespace EventManager.Infrastructure;

public class PaginatorParamException : ArgumentException
{
    public PaginatorParamException(string paramName, int paramValue)
        : this(paramName, paramValue, null) { }

    public PaginatorParamException(string paramName, int paramValue, Exception? innerException)
        : base("Неверное значение параметра разбивки на страницы", paramName, innerException)
    {
        ParamValue = paramValue;
    }
    public int ParamValue { get; }
}