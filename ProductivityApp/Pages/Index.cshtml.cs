using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductivityApp.Data;
using ProductivityApp.Models;

namespace ProductivityApp.Pages;

public class IndexModel(AppDbContext db) : AppPageModel
{
    public string DisplayName { get; set; } = string.Empty;

    public Dictionary<string, int> Summary { get; set; } = [];

    public List<TaskItem> RecentTasks { get; set; } = [];

    public List<Habit> Habits { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null)
        {
            return redirect;
        }

        DisplayName = HttpContext.Session.GetString("DisplayName") ?? "Kullanıcı";
        RecentTasks = await db.Tasks
            .Where(task => task.UserId == UserId && !task.IsDeleted)
            .OrderByDescending(task => task.CreatedAt)
            .Take(4)
            .ToListAsync();

        Habits = await db.Habits
            .Where(habit => habit.UserId == UserId)
            .OrderByDescending(habit => habit.Streak)
            .Take(4)
            .ToListAsync();

        var todayStart = DateTime.Today.ToUniversalTime();
        var tomorrowStart = DateTime.Today.AddDays(1).ToUniversalTime();

        Summary = new Dictionary<string, int>
        {
            ["activeTasks"] = await db.Tasks.CountAsync(task => task.UserId == UserId && !task.IsDeleted && task.Status != TaskProgressStatus.Completed),
            ["habits"] = await db.Habits.CountAsync(habit => habit.UserId == UserId),
            ["focusSessions"] = await db.FocusSessions.CountAsync(session => session.UserId == UserId),
            ["todayFocusMinutes"] = await db.FocusSessions
                .Where(session => session.UserId == UserId && session.CompletedAt >= todayStart && session.CompletedAt < tomorrowStart)
                .SumAsync(session => (int?)session.DurationMinutes) ?? 0,
            ["maxStreak"] = await db.Habits.Where(habit => habit.UserId == UserId).MaxAsync(habit => (int?)habit.Streak) ?? 0
        };

        return Page();
    }

    public static string StatusLabel(TaskProgressStatus status)
    {
        return status switch
        {
            TaskProgressStatus.Waiting => "Bekliyor",
            TaskProgressStatus.InProgress => "Devam ediyor",
            TaskProgressStatus.Completed => "Tamamlandı",
            _ => status.ToString()
        };
    }
}
