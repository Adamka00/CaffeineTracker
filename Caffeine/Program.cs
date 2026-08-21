using System.Globalization;
using Caffeine.Data;
using Caffeine.Repositories;
using Caffeine.Services;
using CaffeineTracker.Repositories;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC Controllerek és View-k hozzáadása
builder.Services.AddControllersWithViews();

// 2. Adatbázis (SQLite) bekötése
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? "Data Source=caffeine.db"));

// 3. Repository-k (Adatelérés) regisztrálása
builder.Services.AddScoped<ICaffeineLogRepository, CaffeineLogRepository>();

// 4. Szervizek (Üzleti logika / Kalkulátor) regisztrálása
builder.Services.AddScoped<ICaffeineDecayStrategy, StandardCaffeineDecayStrategy>();
builder.Services.AddScoped<ICaffeineCalculatorService, CaffeineCalculatorService>();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

var app = builder.Build();

var supportedCultures = new[] { "hu", "en" };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("hu"),
    SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList(),
    SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList()
};
app.UseRequestLocalization(localizationOptions);

// ... (Köztes rétegek: app.UseHttpsRedirection(), app.UseStaticFiles(), stb.)
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tracker}/{action=Index}/{id?}");

app.Run();