using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductivityApp.Data;
using ProductivityApp.Services;

namespace ProductivityApp.Pages;

public class LoginModel(AppDbContext db, PasswordService passwordService) : AppPageModel
{
    [BindProperty]
    public LoginInput Input { get; set; } = new();

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
        var user = await db.Users.FirstOrDefaultAsync(user => user.UserName == userName);

        if (user is null || !passwordService.VerifyPassword(Input.Password, user.PasswordHash))
        {
            ErrorMessage = "Kullanıcı adı veya şifre hatalı.";
            return Page();
        }

        SignIn(user.Id, user.DisplayName);
        return RedirectToPage("/Index");
    }

    public class LoginInput
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
