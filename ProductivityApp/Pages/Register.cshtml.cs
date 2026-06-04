using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductivityApp.Data;
using ProductivityApp.Models;
using ProductivityApp.Services;

namespace ProductivityApp.Pages;

public class RegisterModel(AppDbContext db, PasswordService passwordService) : AppPageModel
{
    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        return CurrentUserId is not null ? RedirectToPage("/Index") : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userName = Input.UserName.Trim();
        if (await db.Users.AnyAsync(user => user.UserName == userName))
        {
            ErrorMessage = "Bu kullanıcı adı zaten kayıtlı.";
            return Page();
        }

        var user = new User
        {
            UserName = userName,
            DisplayName = Input.DisplayName.Trim(),
            PasswordHash = passwordService.HashPassword(Input.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        SignIn(user.Id, user.DisplayName);
        return RedirectToPage("/Index");
    }

    public class RegisterInput
    {
        [Required, MaxLength(80)]
        public string DisplayName { get; set; } = string.Empty;

        [Required, MaxLength(40)]
        public string UserName { get; set; } = string.Empty;

        [Required, MinLength(4)]
        public string Password { get; set; } = string.Empty;
    }
}
