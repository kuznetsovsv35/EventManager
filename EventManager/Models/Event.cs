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

    public int TotalSeats { get; internal set; }

    public int AvailableSeats { get; private set; }

    public bool TryReserveSeats(int count = 1)
    {
        if (AvailableSeats - count < 0)
            return false;
        
        AvailableSeats -= count;
        return true;
    }

    public void ReleaseSeats(int count = 1) => AvailableSeats = Math.Min(AvailableSeats, TotalSeats);

    Event() {}

    public Event(int totalSeats) : this() 
    { 
        Id = Guid.NewGuid();
        TotalSeats = totalSeats; 
        AvailableSeats = TotalSeats;
    }
}