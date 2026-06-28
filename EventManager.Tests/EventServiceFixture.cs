using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Models;
using EventManager.Data;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Tests;

public class EventServiceFixture
{
    public IEventService Service { get; }
    public AppDbContext DbContext { get; }
    
    public EventServiceFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        DbContext = new AppDbContext(options);
        Service = new EventService(DbContext, new FilterService<Event>(), new PaginateService<Event>());
    }
}
