namespace TaskManager.Api.Models;

using System.ComponentModel.DataAnnotations;

public class TaskItem
{
    public int Id { get; set; }

    [Required]
    [MinLength(3)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [RegularExpression("^(pending|in_progress|done)$")]
    public string Status { get; set; } = "pending"; // pending, in_progress, done

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? UserId { get; set; }
}