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
                Priority = TaskPriority.High,
                Status = TaskProgressStatus.Waiting,
                DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                UserId = admin.Id
            },
            new TaskItem
            {
                Title = "E-postalari yanitla",
                Priority = TaskPriority.Normal,
                Status = TaskProgressStatus.InProgress,
                DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                UserId = admin.Id
            });

        db.Habits.AddRange(
            new Habit
            {
                Name = "Sabah egzersizi",
                Streak = 5,
                LastCompletedOn = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                UserId = admin.Id
            },
            new Habit
            {
                Name = "Kitap okuma",
                Streak = 12,
                LastCompletedOn = DateOnly.FromDateTime(DateTime.Today),
                UserId = admin.Id
            });

        db.FocusSessions.Add(new FocusSession
        {
            DurationMinutes = 25,
            CompletedAt = DateTime.UtcNow.AddDays(-1),
            UserId = admin.Id
        });

        await db.SaveChangesAsync();
    }
}
