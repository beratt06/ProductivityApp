using System.ComponentModel.DataAnnotations;

namespace ProductivityApp.Models;

public enum TaskPriority
{
    Low = 1,
    Normal = 2,
    High = 3
}

public enum TaskProgressStatus
{
    Waiting = 1,
    InProgress = 2,
    Completed = 3
}

public class TaskItem
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(40)]
    public string? Category { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Normal;

    public TaskProgressStatus Status { get; set; } = TaskProgressStatus.Waiting;

    public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }
}
