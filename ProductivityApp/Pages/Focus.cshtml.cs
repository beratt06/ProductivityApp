using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductivityApp.Data;
using ProductivityApp.Models;

namespace ProductivityApp.Pages;

public class FocusModel(AppDbContext db) : AppPageModel
{
    public List<FocusSession> Sessions { get; set; } = [];

    public int TodayFocusedMinutes { get; set; }

    public int WeekFocusedMinutes { get; set; }

    public int TodaySessionCount { get; set; }

    public int WeekSessionCount { get; set; }

    public int FocusStreakDays { get; set; }

    public int MaxSessionMinutes { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null)
        {
            return redirect;
        }

        await LoadSessionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCompleteAsync(int durationMinutes)
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null)
        {
            return redirect;
        }

        if (durationMinutes is >= 1 and <= 180)
        {
            db.FocusSessions.Add(new FocusSession
            {
                DurationMinutes = durationMinutes,
                CompletedAt = DateTime.UtcNow,
                UserId = UserId
            });
            await db.SaveChangesAsync();
        }

        return RedirectToPage("/Focus");
    }

    private async Task LoadSessionsAsync()
    {
        var todayStart = DateTime.Today.ToUniversalTime();
        var tomorrowStart = DateTime.Today.AddDays(1).ToUniversalTime();
        var weekStartLocal = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek + 6) % 7);
        var weekStart = weekStartLocal.ToUniversalTime();

        Sessions = await db.FocusSessions
            .Where(session => session.UserId == UserId)
            .OrderByDescending(session => session.CompletedAt)
            .Take(10)
            .ToListAsync();

        TodayFocusedMinutes = await db.FocusSessions
            .Where(session => session.UserId == UserId && session.CompletedAt >= todayStart && session.CompletedAt < tomorrowStart)
            .SumAsync(session => (int?)session.DurationMinutes) ?? 0;

        TodaySessionCount = await db.FocusSessions
            .CountAsync(session => session.UserId == UserId && session.CompletedAt >= todayStart && session.CompletedAt < tomorrowStart);

        WeekFocusedMinutes = await db.FocusSessions
            .Where(session => session.UserId == UserId && session.CompletedAt >= weekStart)
            .SumAsync(session => (int?)session.DurationMinutes) ?? 0;

        WeekSessionCount = await db.FocusSessions
            .CountAsync(session => session.UserId == UserId && session.CompletedAt >= weekStart);

        MaxSessionMinutes = await db.FocusSessions
            .Where(session => session.UserId == UserId)
            .MaxAsync(session => (int?)session.DurationMinutes) ?? 0;

        FocusStreakDays = await CalculateFocusStreakAsync();
    }

    private async Task<int> CalculateFocusStreakAsync()
    {
        var days = await db.FocusSessions
            .Where(session => session.UserId == UserId)
            .Select(session => session.CompletedAt)
            .ToListAsync();

        if (days.Count == 0)
        {
            return 0;
        }

        var daySet = new HashSet<DateOnly>(days.Select(day => DateOnly.FromDateTime(day.ToLocalTime())));
        var cursor = DateOnly.FromDateTime(DateTime.Today);
        var streak = 0;

        while (daySet.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }
}
