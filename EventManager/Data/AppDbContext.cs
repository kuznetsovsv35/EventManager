using EventManager.Application.Interfaces;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data;

/// <summary>
/// Контекст хранения данных события.
/// </summary>
/// <param name="options"></param>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Event> Events { get; set; }

    IQueryable<Event> IAppDbContext.Events => Events;

    public void Add(Event @event)
    {
        Events.Add(@event);
        SaveChanges();
    }

    public void Delete(Event @event)
    {
        Events.Remove(@event);
        SaveChanges();
    }

    public void Update(Event @event)
    {
        Events.Update(@event);
        SaveChanges();
    }
}