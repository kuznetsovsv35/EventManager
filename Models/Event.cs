using System.ComponentModel.DataAnnotations;

namespace EventManager.Models;

public class Event
{
    [Required]
    public required Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public required string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public required DateTime StartAt { get; set; } = DateTime.Now;
    
    [Required]
    public DateTime EndAt { get; set; } = DateTime.Now.AddMinutes(30);
}