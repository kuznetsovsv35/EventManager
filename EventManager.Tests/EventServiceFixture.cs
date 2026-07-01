using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Models;

namespace EventManager.Tests;

public class EventServiceFixture : TestAppDbContext
{
    public IEventService EventService { get; }

    public EventServiceFixture()
        => EventService = new EventService(
            this, 
            new FilterService<Event>(), 
            new PaginateService<Event>());
}
