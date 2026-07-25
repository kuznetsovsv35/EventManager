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

    public bool TryReserveSeats(int count = 1)
    {
        if (AvailableSeats - count < 0)
            return false;
        
        ReservedCount += count;
        return true;
    }

    public void ReleaseSeats(int count = 1) => ReservedCount -= Math.Min(ReservedCount, count);

    public void UpdateTotalSeats(int totalSeats) => TotalSeats = Math.Max(totalSeats, ReservedCount);

    Event() {}

    public Event(int totalSeats) : this() 
    { 
        Id = Guid.NewGuid();
        TotalSeats = totalSeats; 
    }
}