using Microsoft.EntityFrameworkCore;
using ProductivityApp.Data;
using ProductivityApp.Services;

var baseDir = AppContext.BaseDirectory;

var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = baseDir,
    WebRootPath = Path.Combine(baseDir, "wwwroot")
});

builder.WebHost.UseKestrelCore();
builder.WebHost.UseUrls(Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://127.0.0.1:5080");

builder.Services.AddRazorPages();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var dbPath = Path.Combine(baseDir, "app.db");
    options.UseSqlite($"Data Source={dbPath}");
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddSingleton<UndoStackService>();

var app = builder.Build();
var isDevelopment = string.Equals(
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    "Development",
    StringComparison.OrdinalIgnoreCase);

if (isDevelopment)
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

await DatabaseSeeder.SeedAsync(app.Services);

app.Run();
