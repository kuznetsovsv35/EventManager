using Microsoft.EntityFrameworkCore;

namespace EventManager.Application.Interfaces;

public interface ISyncDataContext<T> where T : class
{
    Task ExecuteActionAsync(Func<DbSet<T>, Task> action, CancellationToken cancellation);
}