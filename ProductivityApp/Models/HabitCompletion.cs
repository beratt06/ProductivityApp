namespace ProductivityApp.Models;

public class HabitCompletion
{
    public int Id { get; set; }

    public DateOnly CompletedOn { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int HabitId { get; set; }

    public Habit? Habit { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }
}
