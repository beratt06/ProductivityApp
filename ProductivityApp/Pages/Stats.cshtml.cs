using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductivityApp.Data;
using ProductivityApp.Models;

namespace ProductivityApp.Pages;

public class StatsModel(AppDbContext db) : AppPageModel
{
    public int[] WeeklyFocusMinutes { get; set; } = new int[7];
    public int[] WeeklyCompletedTasks { get; set; } = new int[7];
    public string[] WeekDayLabels { get; set; } = new string[7];

    public List<Habit> Habits { get; set; } = [];
    public Dictionary<int, HashSet<DateOnly>> HabitCompletionDays { get; set; } = [];
    public List<DateOnly> Last30Days { get; set; } = [];

    public int TotalFocusMinutesAllTime { get; set; }
    public int TotalTasksCompleted { get; set; }
    public int BestHabitStreak { get; set; }
    public int TotalHabitCompletions30Days { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null) return redirect;

        await LoadStatsAsync();
        return Page();
    }

    private async Task LoadStatsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        // --- Week day labels (last 7 days) ---
        var dayNames = new[] { "Paz", "Pzt", "Sal", "Çar", "Per", "Cum", "Cmt" };
        for (int i = 0; i < 7; i++)
        {
            var day = DateTime.Today.AddDays(i - 6);
            WeekDayLabels[i] = dayNames[(int)day.DayOfWeek];
        }

        // --- Focus data per day ---
        var weekStartUtc = DateTime.Today.AddDays(-6).ToUniversalTime();
        var allFocusSessions = await db.FocusSessions
            .Where(s => s.UserId == UserId && s.CompletedAt >= weekStartUtc)
            .ToListAsync();

        for (int i = 0; i < 7; i++)
        {
            var targetDay = DateTime.Today.AddDays(i - 6);
            var dayStartUtc = targetDay.ToUniversalTime();
            var dayEndUtc = targetDay.AddDays(1).ToUniversalTime();
            WeeklyFocusMinutes[i] = allFocusSessions
                .Where(s => s.CompletedAt >= dayStartUtc && s.CompletedAt < dayEndUtc)
                .Sum(s => s.DurationMinutes);
        }

        // --- Completed tasks per day (via UpdatedAt) ---
        var weekStartUtcForTasks = DateTime.Today.AddDays(-6).ToUniversalTime();
        var recentCompleted = await db.Tasks
            .Where(t => t.UserId == UserId && !t.IsDeleted
                && t.Status == TaskProgressStatus.Completed
                && t.UpdatedAt >= weekStartUtcForTasks)
            .ToListAsync();

        for (int i = 0; i < 7; i++)
        {
            var targetDay = DateTime.Today.AddDays(i - 6);
            var dayStartUtc = targetDay.ToUniversalTime();
            var dayEndUtc = targetDay.AddDays(1).ToUniversalTime();
            WeeklyCompletedTasks[i] = recentCompleted
                .Count(t => t.UpdatedAt >= dayStartUtc && t.UpdatedAt < dayEndUtc);
        }

        // --- Habits heatmap (last 30 days) ---
        Last30Days = Enumerable.Range(0, 30)
            .Select(i => today.AddDays(-29 + i))
            .ToList();

        Habits = await db.Habits
            .Where(h => h.UserId == UserId)
            .OrderByDescending(h => h.Streak)
            .ThenBy(h => h.Name)
            .ToListAsync();

        var thirtyDayStart = today.AddDays(-29);
        var completions = await db.HabitCompletions
            .Where(c => c.UserId == UserId && c.CompletedOn >= thirtyDayStart && c.CompletedOn <= today)
            .ToListAsync();

        HabitCompletionDays = Habits.ToDictionary(
            h => h.Id,
            h => new HashSet<DateOnly>(completions
                .Where(c => c.HabitId == h.Id)
                .Select(c => c.CompletedOn))
        );

        TotalHabitCompletions30Days = completions.Count;

        // --- Summary ---
        TotalFocusMinutesAllTime = await db.FocusSessions
            .Where(s => s.UserId == UserId)
            .SumAsync(s => (int?)s.DurationMinutes) ?? 0;

        TotalTasksCompleted = await db.Tasks
            .CountAsync(t => t.UserId == UserId && !t.IsDeleted && t.Status == TaskProgressStatus.Completed);

        BestHabitStreak = Habits.Count > 0 ? Habits.Max(h => h.Streak) : 0;
    }
}
