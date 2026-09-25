using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Caffeine.Data;
using Caffeine.Repositories;
using Caffeine.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(
        new AutoValidateAntiforgeryTokenAttribute()));

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton(
    TimeProvider.System);

builder.Services.AddScoped<TrackerClock>();

builder.Services.AddScoped<StreakService>();

// Koffi 3.0: calculations and persistence
// stay out of TrackerController.
builder.Services.AddScoped<PreferenceService>();

builder.Services.AddScoped<SleepService>();

builder.Services.AddScoped<SleepInsightService>();

builder.Services.AddScoped<CaffeinePlanningService>();

builder.Services.AddScoped<DrinkCatalog>();

builder.Services.AddScoped<LifetimeStatsService>();

builder.Services.AddScoped<PasswordResetService>();

builder.Services.AddSingleton<PushConfiguration>();

builder.Services.AddScoped<PushSubscriptionService>();

builder.Services.AddScoped<NotificationService>();

builder.Services
    .AddHttpClient<IPushSender, WebPushSender>(
        client =>
            client.Timeout =
                TimeSpan.FromSeconds(15))
    .ConfigurePrimaryHttpMessageHandler(
        () =>
            new HttpClientHandler
            {
                AllowAutoRedirect = false
            });

builder.Services.AddHostedService<
    NotificationWorker>();

builder.Services.AddScoped<
    AccountCookieEvents>();

builder.Services.AddSingleton<
    IEmailSender,
    SmtpEmailSender>();

builder.Services.AddSingleton<
    ResetMailQueue>();

builder.Services.AddHostedService<
    ResetEmailWorker>();


builder.Services.AddRateLimiter(
    options =>
    {
        options.RejectionStatusCode = 429;

        options.AddPolicy(
            "push-subscription",
            context =>
                RateLimitPartition
                    .GetFixedWindowLimiter(
                        context.Connection
                            .RemoteIpAddress?
                            .ToString()
                        ?? "unknown",
                        _ =>
                            new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 20,

                                Window =
                                    TimeSpan
                                        .FromMinutes(1),

                                QueueLimit = 0
                            }));

        options.AddPolicy(
            "password-reset",
            context =>
                RateLimitPartition
                    .GetFixedWindowLimiter(
                        context.Connection
                            .RemoteIpAddress?
                            .ToString()
                        ?? "unknown",
                        _ =>
                            new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 5,

                                Window =
                                    TimeSpan
                                        .FromMinutes(1),

                                QueueLimit = 0
                            }));
    });


builder.Services.Configure<
    ForwardedHeadersOptions>(
    options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor |
            ForwardedHeaders.XForwardedProto;

        foreach (
            var value in
            builder.Configuration
                .GetSection(
                    "ReverseProxy:KnownProxies")
                .Get<string[]>()
            ?? [])
        {
            if (System.Net.IPAddress.TryParse(
                    value,
                    out var address))
            {
                options.KnownProxies.Add(
                    address);
            }
        }
    });


builder.Services.AddScoped<
    CurrentTrackerUser>();


var dataProtection =
    builder.Services
        .AddDataProtection()
        .SetApplicationName(
            "CaffeineTracker");

var keyPath =
    builder.Configuration[
        "DataProtection:KeyPath"];

if (!string.IsNullOrWhiteSpace(
        keyPath))
{
    dataProtection
        .PersistKeysToFileSystem(
            new DirectoryInfo(
                keyPath));
}


builder.Services.AddDbContext<
    AppDbContext>(
    options =>
        options.UseSqlite(
            builder.Configuration
                .GetConnectionString(
                    "DefaultConnection")
            ?? "Data Source=caffeine.db"));


builder.Services.AddScoped<
    ICaffeineLogRepository,
    CaffeineLogRepository>();


builder.Services.AddScoped<
    ICaffeineDecayStrategy,
    StandardCaffeineDecayStrategy>();

builder.Services.AddScoped<
    ICaffeineCalculatorService,
    CaffeineCalculatorService>();


builder.Services.AddLocalization(
    options =>
        options.ResourcesPath =
            "Resources");


builder.Services
    .AddControllersWithViews()
    .AddViewLocalization(
        Microsoft.AspNetCore.Mvc.Razor
            .LanguageViewLocationExpanderFormat
            .Suffix)
    .AddDataAnnotationsLocalization();


builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults
            .AuthenticationScheme)
    .AddCookie(
        options =>
        {
            options.EventsType =
                typeof(AccountCookieEvents);

            options.LoginPath =
                "/Account/Login";

            options.LogoutPath =
                "/Account/Logout";

            options.Cookie.Name =
                "CaffeineAuth";

            options.ExpireTimeSpan =
                TimeSpan.FromDays(30);
        });


var app = builder.Build();


if (args.Contains("--migrate"))
{
    using var scope =
        app.Services.CreateScope();

    await DatabaseUpgrade.ApplyAsync(
        scope.ServiceProvider
            .GetRequiredService<
                AppDbContext>());

    return;
}


var supportedCultures =
    new[]
    {
        "hu",
        "en"
    };


var localizationOptions =
    new RequestLocalizationOptions
    {
        DefaultRequestCulture =
            new RequestCulture("hu"),

        SupportedCultures =
            supportedCultures
                .Select(
                    c =>
                        new CultureInfo(c))
                .ToList(),

        SupportedUICultures =
            supportedCultures
                .Select(
                    c =>
                        new CultureInfo(c))
                .ToList()
    };


app.UseForwardedHeaders();

app.UseRequestLocalization(
    localizationOptions);


app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();


app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Tracker}/{action=Index}/{id?}");


app.Run();


public partial class Program
{
}