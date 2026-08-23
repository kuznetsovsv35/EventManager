using System.Security.Cryptography;

namespace EventManager.Models;

/// <summary>
/// Модель данных события.
/// </summary>
public class Event
{
    public Guid Id { get; private set; }

    public required string Title { get; set; }

    public string? Description { get; set; }

    public required DateTime StartAt { get; set; }

    public required DateTime EndAt { get; set; }

    public int TotalSeats { get; private set; }

    public int ReservedCount { get; private set; }

    public int AvailableSeats => TotalSeats - ReservedCount;

    public List<Booking> Bookings { get; set; } = null!;

    /// <summary>
    /// Попытка забронировать места.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public bool TryReserveSeats(int count = 1)
    {
        if (AvailableSeats < count)
            return false;

        ReservedCount += count;
        return true;
    }

    /// <summary>
    /// Освободить места.
    /// </summary>
    /// <param name="count"></param>
    public void ReleaseSeats(int count = 1) => ReservedCount -= Math.Min(ReservedCount, count);

    /// <summary>
    /// Обновить общее количество мест.
    /// </summary>
    /// <param name="totalSeats"></param>
    public void UpdateTotalSeats(int totalSeats) => TotalSeats = Math.Max(totalSeats, ReservedCount);

    /// <summary>
    /// Приватный конструктор без параметров.
    /// </summary>
    Event() { }

    /// <summary>
    /// Конструктор события.
    /// </summary>
    /// <param name="totalSeats"></param>
    public Event(int totalSeats) : this()
    {
        Id = Guid.NewGuid();
        TotalSeats = totalSeats;
    }
}