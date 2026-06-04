using System.ComponentModel.DataAnnotations;

namespace ProductivityApp.Models;

public class Habit
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int Streak { get; set; }

    public DateOnly? LastCompletedOn { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }

    public User? User { get; set; }

    public List<HabitCompletion> Completions { get; set; } = [];
}
