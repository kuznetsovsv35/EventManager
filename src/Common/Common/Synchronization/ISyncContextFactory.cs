namespace Common.Synchronization;

/// <summary>
/// Фабрика контекста синхронизации
/// </summary>
public interface ISyncContextFactory
{
    ISyncContext CreateContext<T>();
}