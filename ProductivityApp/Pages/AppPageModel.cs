using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProductivityApp.Pages;

public abstract class AppPageModel : PageModel
{
    protected int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

    protected int UserId => CurrentUserId ?? 0;

    protected IActionResult? RedirectIfAnonymous()
    {
        return CurrentUserId is null ? RedirectToPage("/Login") : null;
    }

    protected void SignIn(int userId, string displayName)
    {
        HttpContext.Session.SetInt32("UserId", userId);
        HttpContext.Session.SetString("DisplayName", displayName);
    }
}
