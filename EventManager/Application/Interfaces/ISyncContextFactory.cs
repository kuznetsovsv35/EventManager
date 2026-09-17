namespace EventManager.Application.Interfaces;

/// <summary>
/// Фабрика контекста синхронизации
/// </summary>
public interface ISyncContextFactory
{
    ISyncContext CreateContext<T>();
}