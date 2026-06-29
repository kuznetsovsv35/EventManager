using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Models;
using EventManager.Data;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Tests;

public class EventServiceFixture
{
    public IEventService AppService { get; }

    public IFilter<Event> FilterService { get; }

    public IPaginator<Event> Paginator { get; }
    
    public AppDbContext DbContext { get; }
    
    public EventServiceFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        DbContext = new TestAppDbContext(options);
        FilterService = new FilterService<Event>();
        Paginator = new PaginateService<Event>();
        AppService = new EventService(DbContext, FilterService, Paginator);
    }
}
