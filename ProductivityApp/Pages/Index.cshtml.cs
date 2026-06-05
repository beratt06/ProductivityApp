using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductivityApp.Data;
using ProductivityApp.Models;

namespace ProductivityApp.Pages;

public class IndexModel(AppDbContext db) : AppPageModel
{
    public string DisplayName { get; set; } = string.Empty;

    public List<TaskItem> RecentTasks { get; set; } = [];
    public List<TaskItem> TodayTasks { get; set; } = [];
    public List<TaskItem> OverdueTasks { get; set; } = [];
    public List<TaskItem> TomorrowTasks { get; set; } = [];
    public List<Habit> Habits { get; set; } = [];
    public List<Habit> UnfinishedHabits { get; set; } = [];

    public int ActiveTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int CompletionRate { get; set; }
    public int DueTodayTasksCount { get; set; }
    public int OverdueTasksCount { get; set; }
    public int TodayFocusMinutes { get; set; }
    public int WeekFocusMinutes { get; set; }
    public int MaxStreak { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null)
            return redirect;

        DisplayName = HttpContext.Session.GetString("DisplayName") ?? "Kullanıcı";

        var today = DateOnly.FromDateTime(DateTime.Today);
        var tomorrow = today.AddDays(1);
        var todayStart = DateTime.Today.ToUniversalTime();
        var tomorrowStart = DateTime.Today.AddDays(1).ToUniversalTime();
        var weekStartLocal = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek + 6) % 7);
        var weekStart = weekStartLocal.ToUniversalTime();

        var tasksQuery = db.Tasks.Where(t => t.UserId == UserId && !t.IsDeleted);

        ActiveTasks = await tasksQuery.CountAsync(t => t.Status != TaskProgressStatus.Completed);
        CompletedTasks = await tasksQuery.CountAsync(t => t.Status == TaskProgressStatus.Completed);
        DueTodayTasksCount = await tasksQuery.CountAsync(t => t.Status != TaskProgressStatus.Completed && t.DueDate == today);
        OverdueTasksCount = await tasksQuery.CountAsync(t => t.Status != TaskProgressStatus.Completed && t.DueDate < today);
        var totalTasks = await tasksQuery.CountAsync();
        CompletionRate = totalTasks == 0 ? 0 : (int)Math.Round(CompletedTasks * 100.0 / totalTasks);

        RecentTasks = await tasksQuery
            .OrderByDescending(t => t.CreatedAt)
            .Take(4)
            .ToListAsync();

        TodayTasks = await tasksQuery
            .Where(t => t.Status != TaskProgressStatus.Completed && t.DueDate == today)
            .OrderByDescending(t => t.Priority)
            .Take(4)
            .ToListAsync();

        OverdueTasks = await tasksQuery
            .Where(t => t.Status != TaskProgressStatus.Completed && t.DueDate < today)
            .OrderBy(t => t.DueDate)
            .Take(4)
            .ToListAsync();

        TomorrowTasks = await tasksQuery
            .Where(t => t.Status != TaskProgressStatus.Completed && t.DueDate == tomorrow)
            .OrderByDescending(t => t.Priority)
            .Take(4)
            .ToListAsync();

        Habits = await db.Habits
            .Where(h => h.UserId == UserId)
            .OrderByDescending(h => h.Streak)
            .Take(4)
            .ToListAsync();

        UnfinishedHabits = await db.Habits
            .Where(h => h.UserId == UserId && h.LastCompletedOn != today)
            .OrderByDescending(h => h.Streak)
            .Take(4)
            .ToListAsync();

        TodayFocusMinutes = await db.FocusSessions
            .Where(s => s.UserId == UserId && s.CompletedAt >= todayStart && s.CompletedAt < tomorrowStart)
            .SumAsync(s => (int?)s.DurationMinutes) ?? 0;

        WeekFocusMinutes = await db.FocusSessions
            .Where(s => s.UserId == UserId && s.CompletedAt >= weekStart)
            .SumAsync(s => (int?)s.DurationMinutes) ?? 0;

        MaxStreak = await db.Habits
            .Where(h => h.UserId == UserId)
            .MaxAsync(h => (int?)h.Streak) ?? 0;

        return Page();
    }

    public static string StatusLabel(TaskProgressStatus status) => status switch
    {
        TaskProgressStatus.Waiting => "Bekliyor",
        TaskProgressStatus.InProgress => "Devam ediyor",
        TaskProgressStatus.Completed => "Tamamlandı",
        _ => status.ToString()
    };

    public static string PriorityLabel(TaskPriority priority) => priority switch
    {
        TaskPriority.Low => "Düşük",
        TaskPriority.Normal => "Normal",
        TaskPriority.High => "Yüksek",
        _ => priority.ToString()
    };
}
