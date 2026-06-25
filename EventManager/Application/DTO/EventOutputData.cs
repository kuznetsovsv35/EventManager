namespace EventManager.Application.DataTransfer;

/// <summary>
/// Выходные данные запросов GET.
/// </summary>
/// <param name="source"></param>
public class EventOutputData: EventInputData
{
    public Guid Id { get; internal init; }
}