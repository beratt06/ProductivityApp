using System.ComponentModel.DataAnnotations;

namespace ProductivityApp.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(40)]
    public string UserName { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<TaskItem> Tasks { get; set; } = [];

    public List<Habit> Habits { get; set; } = [];

    public List<FocusSession> FocusSessions { get; set; } = [];
}
