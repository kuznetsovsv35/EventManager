using EventManager.Common.Interfaces;
using EventManager.Common.Private;

namespace EventManager.Common.Services;

public class SyncContextFactory : ISyncContextFactory
{
    ISyncContext ISyncContextFactory.CreateContext<T>()
        => new SyncDataContext<T>();
}