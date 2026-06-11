using EventManager.Models;

namespace EventManager.Application.DataTransfer;

public class EventInputData
{
    public string? Title { get; set; }
    
    public string? Description { get; set; }
    
    public DateTime StartAt { get; set; }
    
    public DateTime EndAt { get; set; }

    public EventInputData() {}

    internal EventInputData(Event source)
    {
        Title = source.Title;
        Description = source.Description;
        StartAt = source.StartAt;
        EndAt = source.EndAt;
    }

    internal Event ToEvent()
    {
        if (string.IsNullOrWhiteSpace(Title))
            throw new ArgumentException(nameof(Title));

        return new Event()
        {
            Id = Guid.NewGuid(),
            Title = this.Title,
            Description = this.Description,
            StartAt = this.StartAt,
            EndAt = this.EndAt
        };
    }

    internal void Update(Event dest)
    {
        if (string.IsNullOrWhiteSpace(Title))
            throw new ArgumentException(nameof(Title));

        dest.Title = Title;
        dest.Description = Description;
        dest.StartAt = StartAt;
        dest.EndAt = EndAt;
    }
}
