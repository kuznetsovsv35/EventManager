using Microsoft.EntityFrameworkCore;
using UsersService.Domain;

namespace UsersService.Database;

public class UsersDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
}
