using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductivityApp.Data;
using ProductivityApp.Models;

namespace ProductivityApp.Pages;

public class HabitsModel(AppDbContext db) : AppPageModel
{
    [BindProperty]
    public HabitInput Input { get; set; } = new();

    public List<Habit> Habits { get; set; } = [];

    public int ActiveHabitsCount { get; set; }
    public int CompletedTodayCount { get; set; }
    public int WeeklyCompletionsCount { get; set; }
    public int BestStreak { get; set; }

    public Dictionary<int, int> WeeklyCounts { get; set; } = [];
    public Dictionary<int, int> WeeklyRates { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null) return redirect;

        await LoadHabitsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null) return redirect;

        if (!ModelState.IsValid)
        {
            await LoadHabitsAsync();
            return Page();
        }

        db.Habits.Add(new Habit
        {
            Name = Input.Name.Trim(),
            Emoji = string.IsNullOrWhiteSpace(Input.Emoji) ? "⚡" : Input.Emoji.Trim(),
            FrequencyPerWeek = Input.FrequencyPerWeek,
            UserId = UserId
        });
        await db.SaveChangesAsync();

        return RedirectToPage("/Habits");
    }

    public async Task<IActionResult> OnPostCompleteAsync(int id)
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null) return redirect;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var habit = await db.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == UserId);
        if (habit is null || habit.LastCompletedOn == today)
            return RedirectToPage("/Habits");

        var yesterday = today.AddDays(-1);
        habit.Streak = habit.LastCompletedOn == yesterday ? habit.Streak + 1 : 1;
        habit.LastCompletedOn = today;
        db.HabitCompletions.Add(new HabitCompletion
        {
            HabitId = habit.Id,
            UserId = UserId,
            CompletedOn = today
        });

        await db.SaveChangesAsync();
        return RedirectToPage("/Habits");
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null) return redirect;

        var habit = await db.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == UserId);
        if (habit is not null)
        {
            db.Habits.Remove(habit);
            await db.SaveChangesAsync();
        }

        return RedirectToPage("/Habits");
    }

    public bool CompletedToday(Habit habit) =>
        habit.LastCompletedOn == DateOnly.FromDateTime(DateTime.Today);

    private async Task LoadHabitsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var weekStart = today.AddDays(-6);

        Habits = await db.Habits
            .Where(h => h.UserId == UserId)
            .OrderByDescending(h => h.Streak)
            .ThenBy(h => h.Name)
            .ToListAsync();

        ActiveHabitsCount = Habits.Count;
        CompletedTodayCount = Habits.Count(h => h.LastCompletedOn == today);
        BestStreak = Habits.Count == 0 ? 0 : Habits.Max(h => h.Streak);

        var completions = await db.HabitCompletions
            .Where(c => c.UserId == UserId && c.CompletedOn >= weekStart && c.CompletedOn <= today)
            .ToListAsync();

        WeeklyCompletionsCount = completions.Count;
        WeeklyCounts = completions
            .GroupBy(c => c.HabitId)
            .ToDictionary(g => g.Key, g => g.Count());

        // Rate is based on frequency goal, not 7
        WeeklyRates = Habits.ToDictionary(
            h => h.Id,
            h =>
            {
                var freq = Math.Max(h.FrequencyPerWeek, 1);
                var count = WeeklyCounts.GetValueOrDefault(h.Id);
                return Math.Min((int)Math.Round((count / (double)freq) * 100), 100);
            });
    }

    public class HabitInput
    {
        [Required(ErrorMessage = "Alışkanlık adı zorunludur."), MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(4)]
        public string Emoji { get; set; } = "⚡";

        [Range(1, 7)]
        public int FrequencyPerWeek { get; set; } = 7;
    }
}
