using Microsoft.EntityFrameworkCore;
using ProductivityApp.Models;

namespace ProductivityApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public DbSet<Habit> Habits => Set<Habit>();

    public DbSet<HabitCompletion> HabitCompletions => Set<HabitCompletion>();

    public DbSet<FocusSession> FocusSessions => Set<FocusSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(user => user.UserName)
            .IsUnique();

        modelBuilder.Entity<TaskItem>()
            .HasOne(task => task.User)
            .WithMany(user => user.Tasks)
            .HasForeignKey(task => task.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Habit>()
            .HasOne(habit => habit.User)
            .WithMany(user => user.Habits)
            .HasForeignKey(habit => habit.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HabitCompletion>()
            .HasIndex(completion => new { completion.HabitId, completion.CompletedOn })
            .IsUnique();

        modelBuilder.Entity<HabitCompletion>()
            .HasOne(completion => completion.Habit)
            .WithMany(habit => habit.Completions)
            .HasForeignKey(completion => completion.HabitId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HabitCompletion>()
            .HasOne(completion => completion.User)
            .WithMany()
            .HasForeignKey(completion => completion.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<FocusSession>()
            .HasOne(session => session.User)
            .WithMany(user => user.FocusSessions)
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
