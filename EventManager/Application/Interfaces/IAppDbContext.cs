using System.Linq.Expressions;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Application.Interfaces;

/// <summary>
/// Интерфейс доступа к данным.
/// </summary>
public interface IAppDbContext : IObjectRepository<Booking, Guid>, IObjectRepository<Event, Guid> 
{
    /// <summary>
    /// Создание контекста синхронизации.
    /// </summary>
    /// <typeparam name="T">Тип сущности.</typeparam>
    /// <returns></returns>
    ISyncDataContext CreateSyncContext<T>();
}