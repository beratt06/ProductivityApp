using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductivityApp.Data;
using ProductivityApp.Models;
using ProductivityApp.Services;

namespace ProductivityApp.Pages;

public class ProfileModel(AppDbContext db, PasswordService passwordService) : AppPageModel
{
    [BindProperty]
    public DisplayNameInput NameInput { get; set; } = new();

    [BindProperty]
    public PasswordInput PwInput { get; set; } = new();

    public string? SuccessMessage { get; set; }
    public string? NameError { get; set; }
    public string? PwError { get; set; }

    public string CurrentDisplayName { get; set; } = string.Empty;
    public string CurrentUserName { get; set; } = string.Empty;
    public DateTime MemberSince { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null) return redirect;

        await LoadUserAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostNameAsync()
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null) return redirect;

        await LoadUserAsync();

        if (!ModelState.IsValid)
            return Page();

        var user = await db.Users.FindAsync(UserId);
        if (user is null)
            return RedirectToPage("/Login");

        user.DisplayName = NameInput.DisplayName.Trim();
        await db.SaveChangesAsync();

        // Update session
        HttpContext.Session.SetString("DisplayName", user.DisplayName);
        CurrentDisplayName = user.DisplayName;
        SuccessMessage = "Görünen ad başarıyla güncellendi.";
        return Page();
    }

    public async Task<IActionResult> OnPostPasswordAsync()
    {
        var redirect = RedirectIfAnonymous();
        if (redirect is not null) return redirect;

        await LoadUserAsync();

        var user = await db.Users.FindAsync(UserId);
        if (user is null)
            return RedirectToPage("/Login");

        if (!passwordService.VerifyPassword(PwInput.CurrentPassword, user.PasswordHash))
        {
            PwError = "Mevcut şifre hatalı.";
            return Page();
        }

        if (PwInput.NewPassword != PwInput.ConfirmPassword)
        {
            PwError = "Yeni şifreler eşleşmiyor.";
            return Page();
        }

        user.PasswordHash = passwordService.HashPassword(PwInput.NewPassword);
        await db.SaveChangesAsync();

        SuccessMessage = "Şifre başarıyla değiştirildi.";
        return Page();
    }

    private async Task LoadUserAsync()
    {
        var user = await db.Users.FindAsync(UserId);
        if (user is null) return;
        CurrentDisplayName = user.DisplayName;
        CurrentUserName = user.UserName;
        MemberSince = user.CreatedAt;
        NameInput.DisplayName = user.DisplayName;
    }

    public class DisplayNameInput
    {
        [Required(ErrorMessage = "Görünen ad zorunludur."), MaxLength(80)]
        public string DisplayName { get; set; } = string.Empty;
    }

    public class PasswordInput
    {
        [Required(ErrorMessage = "Mevcut şifre zorunludur.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre zorunludur."), MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
