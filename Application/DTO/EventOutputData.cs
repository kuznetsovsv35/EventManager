using EventManager.Models;

namespace EventManager.Application.DataTransfer;

public class EventOutputData(Event source) : EventInputData(source)
{
    public Guid Id { get; } = source.Id;
}