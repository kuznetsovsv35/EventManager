using EventManager.Common.Interfaces;

namespace EventManager.Common.Services;

public class SyncContextFactory : ISyncContextFactory
{
    ISyncContext ISyncContextFactory.CreateContext<T>()
        => new SyncDataContext<T>();
}