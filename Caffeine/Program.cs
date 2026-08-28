using System.Globalization;
using Caffeine.Data;
using Caffeine.Repositories;
using Caffeine.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=caffeine.db"));


builder.Services.AddScoped<ICaffeineLogRepository, CaffeineLogRepository>();


builder.Services.AddScoped<ICaffeineDecayStrategy, StandardCaffeineDecayStrategy>();
builder.Services.AddScoped<ICaffeineCalculatorService, CaffeineCalculatorService>();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.Cookie.Name = "CaffeineAuth";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
    });

var app = builder.Build();

var supportedCultures = new[] { "hu", "en" };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("hu"),
    SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList(),
    SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList()
};
app.UseRequestLocalization(localizationOptions);


app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tracker}/{action=Index}/{id?}");

app.Run();