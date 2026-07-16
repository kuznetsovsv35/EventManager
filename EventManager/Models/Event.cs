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

    public int AvailableSeats { get; private set; }

     Event() {}

    public Event(int totalSeats) : this() 
    { 
        Id = Guid.NewGuid();
        TotalSeats = totalSeats; 
    }
}