using EventManager.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data;

/// <summary>
/// Контекст хранения данных события.
/// </summary>
/// <param name="options"></param>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    #region Общие
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    #endregion

    #region  Events
    public DbSet<Event> Events => Set<Event>();
    #endregion

    #region  Bookings
    public DbSet<Booking> Bookings => Set<Booking>();
    #endregion
}