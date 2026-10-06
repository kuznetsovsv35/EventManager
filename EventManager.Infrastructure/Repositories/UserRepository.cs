using System.Linq.Expressions;
using EventManager.Application.Interfaces;
using EventManager.Database;
using EventManager.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<User> AddUserAsync(User user, CancellationToken cancellation)
    {
        dbContext.Add(user);
        await dbContext.SaveChangesAsync(cancellation);
        return user;
    }

    public Task<User?> DeleteUserAsync(Guid id, CancellationToken cancellation)
        => DeleteUserAsync(u => u.Id == id, cancellation);

    public Task<User?> DeleteUserAsync(string login, CancellationToken cancellation)
        => DeleteUserAsync(u => u.Login == login, cancellation);

    public Task<User?> GetUserAsync(string login, CancellationToken cancellation)
        => GetUserAsync(u => u.Login == login, cancellation);

    public Task<User?> GetUserAsync(Guid id, CancellationToken cancellation)
        => GetUserAsync(u => u.Id == id, cancellation);

    public Task<User?> UpdateUserAsync(Guid id, Action<User> action, CancellationToken cancellation)
        => UpdateUserAsync(u => u.Id == id, action, cancellation);

    public Task<User?> UpdateUserAsync(string login, Action<User> action, CancellationToken cancellation)
        => UpdateUserAsync(u => u.Login == login, action, cancellation);

    Task<User?> GetUserAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellation)
        => dbContext.Users
        .AsNoTracking()
        .Where(predicate)
        .AsSplitQuery()
        .Where(u => u.IsActive)
        .SingleOrDefaultAsync(cancellation);

    Task<User?> DeleteUserAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellation)
        => UpdateUserAsync(predicate, u => u.Delete(), cancellation);

    async Task<User?> UpdateUserAsync(Expression<Func<User, bool>> predicate, Action<User> action, CancellationToken cancellation)
    {
        if (await GetUserAsync(predicate, cancellation) is User user)
        {
            action?.Invoke(user);
            await dbContext.SaveChangesAsync(cancellation);
            return user;
        }

        return null;
    }
}