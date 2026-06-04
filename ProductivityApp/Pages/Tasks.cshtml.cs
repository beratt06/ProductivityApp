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

    [BindProperty]
    public TaskInput Input { get; set; } = new();

    public List<TaskItem> Tasks { get; set; } = [];

    public bool CanUndo { get; set; }

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

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null)
        {
            return redirect;
        }

        await LoadTasksAsync();
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
            await LoadTasksAsync();
            return Page();
        }

        db.Tasks.Add(new TaskItem
        {
            Title = Input.Title.Trim(),
            Priority = Input.Priority,
            Status = Input.Status,
            DueDate = DateOnly.FromDateTime(Input.DueDate),
            UserId = UserId
        });
        await db.SaveChangesAsync();

        return RedirectToPage("/Tasks");
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id, string title, TaskPriority priority, TaskProgressStatus status, DateTime dueDate)
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null)
        {
            return redirect;
        }

        var task = await db.Tasks.FirstOrDefaultAsync(task => task.Id == id && task.UserId == UserId && !task.IsDeleted);
        if (task is not null && !string.IsNullOrWhiteSpace(title))
        {
            task.Title = title.Trim();
            task.Priority = priority;
            task.Status = status;
            task.DueDate = DateOnly.FromDateTime(dueDate);
            task.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        return RedirectToPage("/Tasks", new { Search, Priority, Status });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null)
        {
            return redirect;
        }

        var task = await db.Tasks.FirstOrDefaultAsync(task => task.Id == id && task.UserId == UserId && !task.IsDeleted);
        if (task is not null)
        {
            task.IsDeleted = true;
            task.DeletedAt = DateTime.UtcNow;
            undoStack.PushDeletedTask(UserId, task.Id);
            await db.SaveChangesAsync();
        }

        return RedirectToPage("/Tasks", new { Search, Priority, Status });
    }

    public async Task<IActionResult> OnPostUndoAsync()
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null)
        {
            return redirect;
        }

        var deletedTaskId = undoStack.PopDeletedTask(UserId);
        if (deletedTaskId is not null)
        {
            var task = await db.Tasks.FirstOrDefaultAsync(task => task.Id == deletedTaskId && task.UserId == UserId && task.IsDeleted);
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
        var query = db.Tasks
            .Where(task => task.UserId == UserId && !task.IsDeleted);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            query = query.Where(task => task.Title.Contains(Search.Trim()));
        }

        if (Priority is not null)
        {
            query = query.Where(task => task.Priority == Priority);
        }

        if (Status is not null)
        {
            query = query.Where(task => task.Status == Status);
        }

        Tasks = await query
            .OrderBy(task => task.Status == TaskProgressStatus.Completed)
            .ThenBy(task => task.DueDate)
            .ThenByDescending(task => task.Priority)
            .ToListAsync();

        CanUndo = undoStack.HasDeletedTask(UserId);
    }

    public class TaskInput
    {
        [Required, MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        public TaskPriority Priority { get; set; } = TaskPriority.Normal;

        public TaskProgressStatus Status { get; set; } = TaskProgressStatus.Waiting;

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Today;
    }
}
