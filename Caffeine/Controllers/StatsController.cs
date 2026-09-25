using Caffeine.Services;
using Microsoft.AspNetCore.Mvc;

namespace Caffeine.Controllers;

public sealed class StatsController(
    CurrentTrackerUser user,
    TrackerClock clock,
    LifetimeStatsService stats)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index() =>
        View(
            await stats.GetAsync(
                user.GetId(),
                clock.Now));
}