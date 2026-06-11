using System.ComponentModel.DataAnnotations;

namespace EventManager.Models;

public class Event
{
    [Required]
    public required Guid Id { get; set; }
    
    [Required]
    public required string Title { get; set; }
    
    public string? Description { get; set; }
    
    [Required]
    public required DateTime StartAt { get; set; }
    
    [Required]
    public DateTime EndAt { get; set; }
}