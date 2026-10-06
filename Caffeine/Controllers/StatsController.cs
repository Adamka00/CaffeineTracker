using System.Globalization;
using Caffeine.Services;
using Caffeine.ViewModels;
using Microsoft.AspNetCore.Mvc;
namespace Caffeine.Controllers;
public sealed class StatsController(CurrentTrackerUser user, TrackerClock clock, LifetimeStatsService stats) : Controller
{
    [HttpGet] public async Task<IActionResult> Index(int? year = null, string? month = null)
    {
        var now = clock.Now;
        if (year.HasValue && (year < 2000 || year > now.Year)) return BadRequest();
        DateTime selectedMonth;
        if (month == null) selectedMonth = new DateTime(year ?? now.Year, year.HasValue && year < now.Year ? 12 : now.Month, 1);
        else if (!DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out selectedMonth) || selectedMonth.Year < 2000 || selectedMonth > now.Date) return BadRequest();
        var start = new DateTime(selectedMonth.Year, 1, 1);
        return View(new StatsViewModel(await stats.GetAsync(user.GetId(), now, year), await stats.YearsAsync(user.GetId(), now), year, selectedMonth, now.Date,
            await stats.CalendarAsync(user.GetId(), selectedMonth, selectedMonth.AddMonths(1), now),
            await stats.CalendarAsync(user.GetId(), start, start.AddYears(1), now)));
    }
}
