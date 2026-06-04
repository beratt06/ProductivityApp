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

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null)
        {
            return redirect;
        }

        await LoadHabitsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null)
        {
            return redirect;
        }

        if (!ModelState.IsValid)
        {
            await LoadHabitsAsync();
            return Page();
        }

        db.Habits.Add(new Habit
        {
            Name = Input.Name.Trim(),
            UserId = UserId
        });
        await db.SaveChangesAsync();

        return RedirectToPage("/Habits");
    }

    public async Task<IActionResult> OnPostCompleteAsync(int id)
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null)
        {
            return redirect;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var habit = await db.Habits.FirstOrDefaultAsync(habit => habit.Id == id && habit.UserId == UserId);
        if (habit is null || habit.LastCompletedOn == today)
        {
            return RedirectToPage("/Habits");
        }

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
        if (redirect is not null)
        {
            return redirect;
        }

        var habit = await db.Habits.FirstOrDefaultAsync(habit => habit.Id == id && habit.UserId == UserId);
        if (habit is not null)
        {
            db.Habits.Remove(habit);
            await db.SaveChangesAsync();
        }

        return RedirectToPage("/Habits");
    }

    public bool CompletedToday(Habit habit)
    {
        return habit.LastCompletedOn == DateOnly.FromDateTime(DateTime.Today);
    }

    private async Task LoadHabitsAsync()
    {
        Habits = await db.Habits
            .Where(habit => habit.UserId == UserId)
            .OrderByDescending(habit => habit.Streak)
            .ThenBy(habit => habit.Name)
            .ToListAsync();
    }

    public class HabitInput
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
