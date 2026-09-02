using EventManager.Models;

namespace EventManager.Application.Interfaces;

public interface IEventRepository : IObjectRepository<Event, Guid>
{
    
}