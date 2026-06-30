using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Models;

namespace EventManager.Tests;

public class EventServiceFixture : TestAppDbContext
{
    public IEventService AppService { get; }

    public IFilter<Event> FilterService { get; }

    public IPaginator<Event> Paginator { get; }
       
    public EventServiceFixture()
    {        
        FilterService = new FilterService<Event>();
        Paginator = new PaginateService<Event>();
        AppService = new EventService(this, FilterService, Paginator);
    }
}
