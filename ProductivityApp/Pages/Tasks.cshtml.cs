using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductivityApp.Data;
using ProductivityApp.Models;
using ProductivityApp.Services;

namespace ProductivityApp.Pages;

public class TasksModel(AppDbContext db, UndoStackService undoStack) : AppPageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public TaskPriority? Priority { get; set; }

    [BindProperty(SupportsGet = true)]
    public TaskProgressStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public TaskDueFilter? Due { get; set; }

    [BindProperty(SupportsGet = true)]
    public TaskSort? Sort { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CategoryFilter { get; set; }

    [BindProperty]
    public TaskInput Input { get; set; } = new();

    public List<TaskItem> Tasks { get; set; } = [];

    public bool CanUndo { get; set; }

    public TaskStats Stats { get; set; } = new();

    public List<string> Categories { get; set; } = [];

    public SelectList PriorityOptions => new(
        new[]
        {
            new { Value = TaskPriority.Low, Text = "Düşük" },
            new { Value = TaskPriority.Normal, Text = "Normal" },
            new { Value = TaskPriority.High, Text = "Yüksek" }
        },
        "Value",
        "Text");

    public SelectList StatusOptions => new(
        new[]
        {
            new { Value = TaskProgressStatus.Waiting, Text = "Bekliyor" },
            new { Value = TaskProgressStatus.InProgress, Text = "Devam Ediyor" },
            new { Value = TaskProgressStatus.Completed, Text = "Tamamlandı" }
        },
        "Value",
        "Text");

    public SelectList DueOptions => new(
        new[]
        {
            new { Value = TaskDueFilter.Today, Text = "Bugün" },
            new { Value = TaskDueFilter.Overdue, Text = "Geciken" },
            new { Value = TaskDueFilter.Upcoming, Text = "Yaklaşan" }
        },
        "Value",
        "Text");

    public SelectList SortOptions => new(
        new[]
        {
            new { Value = TaskSort.DueDate, Text = "Son Tarih" },
            new { Value = TaskSort.Priority, Text = "Öncelik" },
            new { Value = TaskSort.Status, Text = "Durum" },
            new { Value = TaskSort.Created, Text = "Oluşturulma" }
        },
        "Value",
        "Text");

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null)
            return redirect;

        await LoadTasksAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null)
            return redirect;

        if (!ModelState.IsValid)
        {
            await LoadTasksAsync();
            return Page();
        }

        db.Tasks.Add(new TaskItem
        {
            Title = Input.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(Input.Description) ? null : Input.Description.Trim(),
            Category = string.IsNullOrWhiteSpace(Input.Category) ? null : Input.Category.Trim(),
            Priority = Input.Priority,
            Status = Input.Status,
            DueDate = DateOnly.FromDateTime(Input.DueDate),
            UserId = UserId
        });
        await db.SaveChangesAsync();

        return RedirectToPage("/Tasks");
    }

    public async Task<IActionResult> OnPostUpdateAsync(
        int id, string title, string? description, string? category,
        TaskPriority priority, TaskProgressStatus status, DateTime dueDate)
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null)
            return redirect;

        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId && !t.IsDeleted);
        if (task is not null && !string.IsNullOrWhiteSpace(title))
        {
            task.Title = title.Trim();
            task.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            task.Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
            task.Priority = priority;
            task.Status = status;
            task.DueDate = DateOnly.FromDateTime(dueDate);
            task.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        return RedirectToPage("/Tasks", new { Search, Priority, Status, Due, Sort, CategoryFilter });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null)
            return redirect;

        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId && !t.IsDeleted);
        if (task is not null)
        {
            task.IsDeleted = true;
            task.DeletedAt = DateTime.UtcNow;
            undoStack.PushDeletedTask(UserId, task.Id);
            await db.SaveChangesAsync();
        }

        return RedirectToPage("/Tasks", new { Search, Priority, Status, Due, Sort, CategoryFilter });
    }

    public async Task<IActionResult> OnPostUndoAsync()
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null)
            return redirect;

        var deletedTaskId = undoStack.PopDeletedTask(UserId);
        if (deletedTaskId is not null)
        {
            var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == deletedTaskId && t.UserId == UserId && t.IsDeleted);
            if (task is not null)
            {
                task.IsDeleted = false;
                task.DeletedAt = null;
                task.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
        }

        return RedirectToPage("/Tasks");
    }

    private async Task LoadTasksAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var baseQuery = db.Tasks.Where(t => t.UserId == UserId && !t.IsDeleted);

        Stats.Total = await baseQuery.CountAsync();
        Stats.Completed = await baseQuery.CountAsync(t => t.Status == TaskProgressStatus.Completed);
        Stats.Active = await baseQuery.CountAsync(t => t.Status != TaskProgressStatus.Completed);
        Stats.Overdue = await baseQuery.CountAsync(t => t.Status != TaskProgressStatus.Completed && t.DueDate < today);
        Stats.DueToday = await baseQuery.CountAsync(t => t.Status != TaskProgressStatus.Completed && t.DueDate == today);
        Stats.CompletionRate = Stats.Total == 0 ? 0 : (int)Math.Round(Stats.Completed * 100.0 / Stats.Total);

        // Distinct categories for filter dropdown
        Categories = await baseQuery
            .Where(t => t.Category != null)
            .Select(t => t.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var query = baseQuery;

        if (!string.IsNullOrWhiteSpace(Search))
            query = query.Where(t => t.Title.Contains(Search.Trim()) || (t.Description != null && t.Description.Contains(Search.Trim())));

        if (Priority is not null)
            query = query.Where(t => t.Priority == Priority);

        if (Status is not null)
            query = query.Where(t => t.Status == Status);

        if (!string.IsNullOrWhiteSpace(CategoryFilter))
            query = query.Where(t => t.Category == CategoryFilter);

        if (Due is not null)
        {
            query = Due switch
            {
                TaskDueFilter.Today => query.Where(t => t.Status != TaskProgressStatus.Completed && t.DueDate == today),
                TaskDueFilter.Overdue => query.Where(t => t.Status != TaskProgressStatus.Completed && t.DueDate < today),
                TaskDueFilter.Upcoming => query.Where(t => t.Status != TaskProgressStatus.Completed && t.DueDate > today),
                _ => query
            };
        }

        query = Sort switch
        {
            TaskSort.DueDate => query.OrderBy(t => t.DueDate).ThenByDescending(t => t.Priority),
            TaskSort.Priority => query.OrderByDescending(t => t.Priority).ThenBy(t => t.DueDate),
            TaskSort.Status => query.OrderBy(t => t.Status).ThenBy(t => t.DueDate),
            TaskSort.Created => query.OrderByDescending(t => t.CreatedAt),
            _ => query.OrderBy(t => t.Status == TaskProgressStatus.Completed)
                      .ThenBy(t => t.DueDate)
                      .ThenByDescending(t => t.Priority)
        };

        Tasks = await query.ToListAsync();
        CanUndo = undoStack.HasDeletedTask(UserId);
    }

    public class TaskInput
    {
        [Required, MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(40)]
        public string? Category { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Normal;

        public TaskProgressStatus Status { get; set; } = TaskProgressStatus.Waiting;

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Today;
    }

    public class TaskStats
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Completed { get; set; }
        public int Overdue { get; set; }
        public int DueToday { get; set; }
        public int CompletionRate { get; set; }
    }

    public enum TaskDueFilter
    {
        Today = 1,
        Overdue = 2,
        Upcoming = 3
    }

    public enum TaskSort
    {
        DueDate = 1,
        Priority = 2,
        Status = 3,
        Created = 4
    }
}
