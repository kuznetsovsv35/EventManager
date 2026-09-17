using EventManager.Models;

namespace EventManager.Application.Interfaces;

/// <summary>
/// Интерфейс репозитория бронирований.
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Сохраняет созданную бронь.
    /// </summary>
    /// <param name="booking"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task AddBookingAsync(Booking booking, CancellationToken cancellation);
    /// <summary>
    /// Возвращает бронь по идентификатору.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<Booking?> GetBookingAsync(Guid id, CancellationToken cancellation);
    /// <summary>
    /// Обновляет состояние брони.
    /// </summary>
    /// <param name="booking"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task UpdateBookingStatusAsync(Booking booking, CancellationToken cancellation);
    /// <summary>
    /// Возвращает все "ожидающие" брони (для пакетной обработки)
    /// </summary>
    /// <param name="chunkSize">Размер пачки для обработки</param>
    /// <param name="cancellation"</param>
    /// <returns></returns>
    IAsyncEnumerable<IEnumerable<Booking>> GetPendingBookingsAsync(int chunkSize, CancellationToken cancellation);
}