using EventManager.Models;

namespace EventManager.Application.DataTransfer;

/// <summary>
/// Выходные данные запросов GET.
/// </summary>
/// <param name="source"></param>
public class EventOutputData(Event source) : EventInputData(source)
{
    public Guid Id { get; } = source.Id;
}