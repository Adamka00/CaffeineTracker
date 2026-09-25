using Caffeine.Data;
using Caffeine.Models;
using Caffeine.Repositories;
using Caffeine.Services;
using Caffeine.ViewModels;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Caffeine.Controllers;

public class TrackerController(
    AppDbContext context,
    ICaffeineLogRepository logs,
    ICaffeineCalculatorService calculator,
    CurrentTrackerUser currentUser,
    TrackerClock clock,
    IStringLocalizer<SharedResource> text,
    StreakService streaks,
    PreferenceService preferences)
    : Controller
{
    private static readonly DateTime Earliest =
        new(2000, 1, 1);


    private IQueryable<Beverage> AvailableDrinks(
        string userId) =>
        context.Beverages.Where(b =>
            (b.OwnerId == null &&
             b.Category != "Custom")
            ||
            b.OwnerId == userId);


    private DateTime? ParseDay(
        string? date)
    {
        if (string.IsNullOrEmpty(date))
            return clock.Now.Date;

        return DateTime.TryParseExact(
                   date,
                   "yyyy-MM-dd",
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.None,
                   out var day)
               &&
               day >= Earliest
               &&
               day <= clock.Now.Date
            ? day
            : null;
    }


    [HttpGet]
    public async Task<IActionResult> Index(
        string? date)
    {
        var selected =
            ParseDay(date);

        if (selected == null)
            return BadRequest();

        var day =
            selected.Value;

        var now =
            clock.Now;

        var end =
            day.AddDays(1);

        var userId =
            currentUser.GetId();


        // Include previous days' residual caffeine,
        // including when browsing history.
        var activeLogs =
            await context.CaffeineLogs
                .AsNoTracking()
                .Include(l => l.Beverage)
                .Where(l =>
                    l.UserId == userId &&
                    l.ConsumedAt < end &&
                    l.ConsumedAt <= now)
                .OrderBy(l => l.ConsumedAt)
                .ToListAsync();


        var todayLogs =
            activeLogs
                .Where(l =>
                    l.ConsumedAt >= day)
                .ToList();


        var preference =
            await preferences.GetAsync(
                userId,
                Request.Cookies["TargetSleepTime"]);

        var target =
            preference.PlannedBedtime;


        if (!TimeOnly.TryParseExact(
                target,
                "HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var sleep))
        {
            sleep =
                new TimeOnly(23, 0);
        }


        var sleepAt =
            now.Date.Add(
                sleep.ToTimeSpan());

        if (sleepAt < now)
            sleepAt =
                sleepAt.AddDays(1);


        var model =
            new DashboardViewModel
            {
                Streak =
                    await streaks.GetAsync(
                        userId,
                        now),

                IsCaffeineFreeDay =
                    await context.CaffeineFreeDays
                        .AsNoTracking()
                        .AnyAsync(d =>
                            d.UserId == userId &&
                            d.Day == day),

                TargetMg =
                    preference.TargetMg,

                SelectedDate =
                    day,

                Today =
                    now.Date,

                TodayLogs =
                    todayLogs,


                TotalConsumedTodayMg =
                    Math.Round(
                        todayLogs.Sum(l =>
                            l.TotalCaffeineMg),
                        1),


                CurrentActiveCaffeineMg =
                    Math.Round(
                        calculator
                            .GetCurrentTotalActiveCaffeine(
                                activeLogs,
                                now),
                        1),


                SleepReadinessTime =
                    day == now.Date
                        ? calculator
                            .EstimateSleepReadiness(
                                activeLogs,
                                now,
                                preference.TargetMg)
                        : null,


                TargetSleepTimeStr =
                    sleep.ToString(
                        "HH:mm",
                        CultureInfo.InvariantCulture),


                CaffeineAtTargetSleepTime =
                    Math.Round(
                        calculator
                            .GetCurrentTotalActiveCaffeine(
                                activeLogs,
                                sleepAt),
                        1),


                Favorites =
                    await context.FavoriteDrinks
                        .AsNoTracking()
                        .Include(f => f.Beverage)
                        .Where(f =>
                            f.UserId == userId &&
                            (
                                (
                                    f.Beverage.OwnerId == null &&
                                    f.Beverage.Category != "Custom"
                                )
                                ||
                                f.Beverage.OwnerId == userId
                            ))
                        .OrderBy(f => f.Id)
                        .ToListAsync()
            };


        for (var i = 0;
             i <= 48;
             i++)
        {
            var point =
                day.AddMinutes(
                    i * 30);

            model.ChartData.Add(
                new ChartDataPoint
                {
                    TimeLabel =
                        i == 48
                            ? "24:00"
                            : point.ToString("HH:mm"),

                    ActiveCaffeine =
                        Math.Round(
                            calculator
                                .GetCurrentTotalActiveCaffeine(
                                    activeLogs,
                                    point),
                            1)
                });
        }


        return View(model);
    }


    [HttpGet]
    public async Task<IActionResult> Week(
        string? date)
    {
        var selected =
            ParseDay(date);

        if (selected == null)
            return BadRequest();

        var day =
            selected.Value;

        var start =
            day.AddDays(
                -((int)day.DayOfWeek + 6) % 7);

        var end =
            start.AddDays(7);

        var userId =
            currentUser.GetId();


        var entries =
            await context.CaffeineLogs
                .AsNoTracking()
                .Include(l => l.Beverage)
                .Where(l =>
                    l.UserId == userId &&
                    l.ConsumedAt >= start &&
                    l.ConsumedAt < end &&
                    l.ConsumedAt <= clock.Now)
                .ToListAsync();


        var freeDays =
            await context.CaffeineFreeDays
                .AsNoTracking()
                .Where(d =>
                    d.UserId == userId &&
                    d.Day >= start &&
                    d.Day < end &&
                    d.Day <= clock.Now.Date)
                .Select(d => d.Day)
                .ToListAsync();


        var model =
            new WeekViewModel
            {
                Start = start,
                Today = clock.Now.Date
            };


        for (var i = 0;
             i < 7;
             i++)
        {
            var dateOfDay =
                start.AddDays(i);

            var items =
                entries
                    .Where(l =>
                        l.ConsumedAt.Date ==
                        dateOfDay)
                    .ToList();

            model.Days.Add(
                new DaySummary
                {
                    Date =
                        dateOfDay,

                    IsCaffeineFree =
                        freeDays.Contains(
                            dateOfDay),

                    Count =
                        items.Count,

                    TotalMg =
                        Math.Round(
                            items.Sum(l =>
                                l.TotalCaffeineMg),
                            1)
                });
        }


        model.MostFrequent =
            entries
                .GroupBy(l =>
                    l.BeverageId)
                .OrderByDescending(g =>
                    g.Count())
                .ThenBy(g =>
                    g.Key)
                .FirstOrDefault()
                ?.First()
                .Beverage;


        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> MarkCaffeineFree(
        string? date)
    {
        var today =
            clock.Now.Date;


        if (ParseDay(date) != today ||
            string.IsNullOrEmpty(date))
        {
            TempData["StreakNotice"] =
                "StreakDayChanged";

            return RedirectToAction(
                nameof(Index));
        }


        TempData["StreakNotice"] =
            await streaks.MarkCaffeineFreeAsync(
                currentUser.GetId(),
                today)
                ? "CaffeineFreeSaved"
                : "CaffeineFreeBlocked";


        return RedirectToAction(
            nameof(Index));
    }


    [HttpPost]
    public async Task<IActionResult> UnmarkCaffeineFree(
        string? date)
    {
        var today =
            clock.Now.Date;


        if (ParseDay(date) != today ||
            string.IsNullOrEmpty(date))
        {
            TempData["StreakNotice"] =
                "StreakDayChanged";

            return RedirectToAction(
                nameof(Index));
        }


        await streaks.UnmarkCaffeineFreeAsync(
            currentUser.GetId(),
            today);

        TempData["StreakNotice"] =
            "CaffeineFreeRemoved";


        return RedirectToAction(
            nameof(Index));
    }


    [HttpPost]
    public async Task<IActionResult> SetTargetSleepTime(
        string time)
    {
        if (!TimeOnly.TryParseExact(
                time,
                "HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _))
        {
            return BadRequest();
        }


        var preference =
            await preferences.GetAsync(
                currentUser.GetId());


        await preferences.SaveAsync(
            currentUser.GetId(),
            time,
            preference.TargetMg);


        Response.Cookies.Append(
            "TargetSleepTime",
            time,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires =
                    DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });


        return RedirectToAction(
            nameof(Index));
    }


    private async Task PopulateDrinks(
        LogDrinkFormViewModel model)
    {
        model.Beverages =
            (await AvailableDrinks(
                    currentUser.GetId())
                .AsNoTracking()
                .ToListAsync())
            .OrderBy(
                BeverageDisplay.Name)
            .ToList();


        model.BeverageOptions =
            model.Beverages
                .Select(b =>
                    new SelectListItem
                    {
                        Value =
                            b.Id.ToString(
                                CultureInfo.InvariantCulture),

                        Text =
                            BeverageDisplay.Name(b)
                    });
    }


    [HttpGet]
    public async Task<IActionResult> LogDrink(
        string? date,
        int? beverageId = null,
        int? amountMl = null)
    {
        var selected =
            ParseDay(date);

        if (selected == null)
            return BadRequest();


        var model =
            new LogDrinkFormViewModel
            {
                ConsumedAt =
                    selected == clock.Now.Date
                        ? clock.Now
                        : selected.Value.AddHours(12),

                AmountMl =
                    amountMl is >= 1 and <= 2000
                        ? amountMl.Value
                        : 250,

                SelectedBeverageId =
                    beverageId
            };


        await PopulateDrinks(model);


        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> LogDrink(
        LogDrinkFormViewModel model)
    {
        if (model.ConsumedAt < Earliest ||
            model.ConsumedAt > clock.Now)
        {
            ModelState.AddModelError(
                nameof(model.ConsumedAt),
                text["InvalidConsumptionTime"]);
        }


        if (model.IsCustomDrink &&
            (
                string.IsNullOrWhiteSpace(
                    model.CustomBeverageName)
                ||
                !model.CustomCaffeinePer100Ml.HasValue
                ||
                !double.IsFinite(
                    model.CustomCaffeinePer100Ml.Value)
            ))
        {
            ModelState.AddModelError(
                "",
                text["InvalidCustomDrink"]);
        }


        if (!model.IsCustomDrink &&
            !model.SelectedBeverageId.HasValue)
        {
            ModelState.AddModelError(
                nameof(model.SelectedBeverageId),
                text["ChooseDrink"]);
        }


        if (!ModelState.IsValid)
        {
            await PopulateDrinks(model);

            return View(model);
        }


        var userId =
            currentUser.GetId();

        Beverage? beverage;


        if (model.IsCustomDrink)
        {
            beverage =
                new Beverage
                {
                    Name =
                        model.CustomBeverageName!
                            .Trim(),

                    Category =
                        "Custom",

                    OwnerId =
                        userId,

                    CaffeinePer100Ml =
                        model.CustomCaffeinePer100Ml!
                            .Value,

                    DefaultPortionMl =
                        model.AmountMl
                };


            context.Beverages.Add(
                beverage);
        }
        else
        {
            beverage =
                await AvailableDrinks(
                        userId)
                    .FirstOrDefaultAsync(
                        b =>
                            b.Id ==
                            model.SelectedBeverageId);


            if (beverage == null)
                return NotFound();
        }


        context.CaffeineLogs.Add(
            new CaffeineLog
            {
                Beverage =
                    beverage,

                ConsumedAmountMl =
                    model.AmountMl,

                ConsumedAt =
                    model.ConsumedAt,

                TotalCaffeineMg =
                    Math.Round(
                        beverage.CaffeinePer100Ml *
                        model.AmountMl /
                        100,
                        1),

                UserId =
                    userId
            });


        if (model.SaveAsFavorite &&
            (
                beverage.Id == 0
                ||
                !await context.FavoriteDrinks
                    .AnyAsync(f =>
                        f.UserId == userId &&
                        f.BeverageId == beverage.Id &&
                        f.AmountMl == model.AmountMl)
            ))
        {
            context.FavoriteDrinks.Add(
                new FavoriteDrink
                {
                    UserId =
                        userId,

                    Beverage =
                        beverage,

                    AmountMl =
                        model.AmountMl
                });
        }


        await streaks.SaveLogChangesAsync();


        return RedirectToAction(
            nameof(Index),
            new
            {
                date =
                    model.ConsumedAt
                        .ToString(
                            "yyyy-MM-dd")
            });
    }


    [HttpPost]
    public async Task<IActionResult> QuickAdd(
        int id)
    {
        var userId =
            currentUser.GetId();


        var favorite =
            await context.FavoriteDrinks
                .Include(f =>
                    f.Beverage)
                .FirstOrDefaultAsync(f =>
                    f.Id == id &&
                    f.UserId == userId);


        if (favorite == null ||
            !await AvailableDrinks(
                    userId)
                .AnyAsync(b =>
                    b.Id ==
                    favorite.BeverageId))
        {
            return NotFound();
        }


        var entry =
            new CaffeineLog
            {
                UserId =
                    userId,

                BeverageId =
                    favorite.BeverageId,

                ConsumedAmountMl =
                    favorite.AmountMl,

                ConsumedAt =
                    clock.Now,

                TotalCaffeineMg =
                    Math.Round(
                        favorite.Beverage
                            .CaffeinePer100Ml *
                        favorite.AmountMl /
                        100,
                        1)
            };


        await logs.AddLogAsync(
            entry);


        TempData["QuickAddedId"] =
            entry.Id;


        return RedirectToAction(
            nameof(Index));
    }


    [HttpPost]
    public async Task<IActionResult> UndoQuickAdd(
        int id)
    {
        var userId =
            currentUser.GetId();


        var entry =
            await context.CaffeineLogs
                .FirstOrDefaultAsync(l =>
                    l.Id == id &&
                    l.UserId == userId);


        if (entry == null)
            return NotFound();


        if (clock.Now -
            entry.ConsumedAt >
            TimeSpan.FromSeconds(30))
        {
            return BadRequest();
        }


        context.CaffeineLogs.Remove(
            entry);

        await context.SaveChangesAsync();


        return RedirectToAction(
            nameof(Index));
    }


    [HttpPost]
    public async Task<IActionResult> RemoveFavorite(
        int id)
    {
        var userId =
            currentUser.GetId();


        var favorite =
            await context.FavoriteDrinks
                .FirstOrDefaultAsync(f =>
                    f.Id == id &&
                    f.UserId == userId);


        if (favorite == null)
            return NotFound();


        context.FavoriteDrinks.Remove(
            favorite);

        await context.SaveChangesAsync();


        return RedirectToAction(
            nameof(Index));
    }


    [HttpPost]
    public async Task<IActionResult> DeleteLog(
        int id,
        string? date)
    {
        await logs.DeleteLogAsync(
            id,
            currentUser.GetId());


        return RedirectToAction(
            nameof(Index),
            new
            {
                date =
                    ParseDay(date)?
                        .ToString(
                            "yyyy-MM-dd")
            });
    }


    [HttpPost]
    public IActionResult SetLanguage(
        string culture,
        string returnUrl)
    {
        if (culture is not ("hu" or "en"))
            return BadRequest();


        Response.Cookies.Append(
            CookieRequestCultureProvider
                .DefaultCookieName,

            CookieRequestCultureProvider
                .MakeCookieValue(
                    new RequestCulture(
                        culture)),

            new CookieOptions
            {
                Expires =
                    DateTimeOffset.UtcNow
                        .AddYears(1),

                IsEssential =
                    true
            });


        return LocalRedirect(
            Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : "/");
    }
}