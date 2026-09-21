using EventManager.Application.Interfaces;

namespace EventManager.Infrastructure.Services;

public class SyncContextFactory : ISyncContextFactory
{
    ISyncContext ISyncContextFactory.CreateContext<T>()
        => new SyncDataContext<T>();
}