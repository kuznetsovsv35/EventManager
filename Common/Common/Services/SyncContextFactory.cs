using Common.Private;
using Common.Synchronization;

namespace Common.Services;

public class SyncContextFactory : ISyncContextFactory
{
    ISyncContext ISyncContextFactory.CreateContext<T>()
        => new SyncDataContext<T>();
}