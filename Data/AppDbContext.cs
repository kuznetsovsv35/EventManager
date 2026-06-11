using Microsoft.EntityFrameworkCore;

namespace EventManager.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}    
}