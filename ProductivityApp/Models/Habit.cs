using System.ComponentModel.DataAnnotations;

namespace ProductivityApp.Models;

public class Habit
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(4)]
    public string Emoji { get; set; } = "⚡";

    public int FrequencyPerWeek { get; set; } = 7;

    public int Streak { get; set; }

    public DateOnly? LastCompletedOn { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }

    public User? User { get; set; }

    public List<HabitCompletion> Completions { get; set; } = [];
}
