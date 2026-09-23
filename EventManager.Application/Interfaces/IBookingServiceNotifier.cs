namespace EventManager.Application.Interfaces;

/// <summary>
/// Коммуникатор между сервисом бронирования и сервисом обработки
/// </summary>
public interface IBookingServiceNotifier
{
    /// <summary>
    /// Подача сигнала о создании брони.
    /// </summary>
    /// <param name="bookingId"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task BookingCreatedAsync(Guid bookingId, CancellationToken cancellation);
    /// <summary>
    /// Ожидания создания брони.
    /// </summary>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<IEnumerable<Guid>?> WaitBookingCreationAsync(CancellationToken cancellation);
}