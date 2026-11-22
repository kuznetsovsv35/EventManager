using EventsService.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventsService.Database;

public class EventsDbContext(DbContextOptions<EventsDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventBooking> EventBookings => Set<EventBooking>();
}
