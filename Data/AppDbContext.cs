using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events {get; set;}
}