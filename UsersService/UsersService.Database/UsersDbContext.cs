using Microsoft.EntityFrameworkCore;
using UsersService.Domain;

namespace UsersService.Database;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
}
