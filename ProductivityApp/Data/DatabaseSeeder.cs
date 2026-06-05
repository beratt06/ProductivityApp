using Microsoft.EntityFrameworkCore;
using ProductivityApp.Models;
using ProductivityApp.Services;

namespace ProductivityApp.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<PasswordService>();

        await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync())
        {
            return;
        }

        var admin = new User
        {
            UserName = "admin",
            DisplayName = "Admin Kullanici",
            PasswordHash = passwordService.HashPassword("admin123")
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();

        db.Tasks.AddRange(
            new TaskItem
            {
                Title = "Rapor yaz",
                Description = "Haftalık performans raporunu hazırla ve müdüre gönder.",
                Category = "İş",
                Priority = TaskPriority.High,
                Status = TaskProgressStatus.Waiting,
                DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                UserId = admin.Id
            },
            new TaskItem
            {
                Title = "E-postaları yanıtla",
                Description = "Gelen kutusundaki bekleyen e-postaları yanıtla.",
                Category = "İletişim",
                Priority = TaskPriority.Normal,
                Status = TaskProgressStatus.InProgress,
                DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                UserId = admin.Id
            },
            new TaskItem
            {
                Title = "Proje sunumu hazırla",
                Description = "Q3 hedefleri için sunum slaytlarını tamamla.",
                Category = "İş",
                Priority = TaskPriority.High,
                Status = TaskProgressStatus.Waiting,
                DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                UserId = admin.Id
            },
            new TaskItem
            {
                Title = "Kitap oku",
                Description = "Atomic Habits kitabını bitir.",
                Category = "Kişisel Gelişim",
                Priority = TaskPriority.Low,
                Status = TaskProgressStatus.Completed,
                DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-3)),
                UpdatedAt = DateTime.UtcNow.AddDays(-3),
                UserId = admin.Id
            });

        db.Habits.AddRange(
            new Habit
            {
                Name = "Sabah egzersizi",
                Emoji = "🏃",
                FrequencyPerWeek = 5,
                Streak = 5,
                LastCompletedOn = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                UserId = admin.Id
            },
            new Habit
            {
                Name = "Kitap okuma",
                Emoji = "📚",
                FrequencyPerWeek = 7,
                Streak = 12,
                LastCompletedOn = DateOnly.FromDateTime(DateTime.Today),
                UserId = admin.Id
            },
            new Habit
            {
                Name = "Meditasyon",
                Emoji = "🧘",
                FrequencyPerWeek = 7,
                Streak = 3,
                LastCompletedOn = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                UserId = admin.Id
            },
            new Habit
            {
                Name = "Su içme (2L)",
                Emoji = "💧",
                FrequencyPerWeek = 7,
                Streak = 8,
                LastCompletedOn = DateOnly.FromDateTime(DateTime.Today),
                UserId = admin.Id
            });

        await db.SaveChangesAsync();

        // Add some habit completions for the heatmap
        var habits = await db.Habits.Where(h => h.UserId == admin.Id).ToListAsync();
        var completions = new List<HabitCompletion>();
        var today = DateOnly.FromDateTime(DateTime.Today);

        foreach (var habit in habits)
        {
            for (int i = 1; i <= 30; i++)
            {
                var day = today.AddDays(-i);
                // Add completions with some randomness for realistic data
                if (i % (habit.FrequencyPerWeek == 7 ? 1 : 2) == 0 || i <= habit.Streak)
                {
                    completions.Add(new HabitCompletion
                    {
                        HabitId = habit.Id,
                        UserId = admin.Id,
                        CompletedOn = day
                    });
                }
            }
        }

        db.HabitCompletions.AddRange(completions);

        db.FocusSessions.AddRange(
            new FocusSession { DurationMinutes = 25, CompletedAt = DateTime.UtcNow.AddDays(-1), UserId = admin.Id },
            new FocusSession { DurationMinutes = 45, CompletedAt = DateTime.UtcNow.AddDays(-1).AddHours(-2), UserId = admin.Id },
            new FocusSession { DurationMinutes = 25, CompletedAt = DateTime.UtcNow.AddDays(-2), UserId = admin.Id },
            new FocusSession { DurationMinutes = 60, CompletedAt = DateTime.UtcNow.AddDays(-3), UserId = admin.Id },
            new FocusSession { DurationMinutes = 25, CompletedAt = DateTime.UtcNow.AddDays(-4), UserId = admin.Id },
            new FocusSession { DurationMinutes = 45, CompletedAt = DateTime.UtcNow.AddDays(-5), UserId = admin.Id },
            new FocusSession { DurationMinutes = 30, CompletedAt = DateTime.UtcNow.AddDays(-6), UserId = admin.Id }
        );

        await db.SaveChangesAsync();
    }
}
