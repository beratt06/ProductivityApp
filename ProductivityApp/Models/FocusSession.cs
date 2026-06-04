namespace ProductivityApp.Models;

public class FocusSession
{
    public int Id { get; set; }

    public int DurationMinutes { get; set; }

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }

    public User? User { get; set; }
}
