using Caffeine.Data;
using Caffeine.Services;
using Caffeine.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Caffeine.Controllers;

public sealed class SleepController(
    AppDbContext db,
    CurrentTrackerUser user,
    TrackerClock clock,
    PreferenceService preferences,
    SleepService service,
    SleepInsightService insights,
    IStringLocalizer<SharedResource> text)
    : FeatureController
{
    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1)
    {
        if (page < 1 || page > 10000)
            return BadRequest();

        var query =
            db.SleepLogs
                .AsNoTracking()
                .Where(s =>
                    s.UserId == user.GetId());

        var rows =
            await query
                .OrderByDescending(s =>
                    s.SleepDate)
                .Skip((page - 1) * 20)
                .Take(21)
                .ToListAsync();

        return View(new SleepIndex
        {
            Logs = rows.Take(20).ToList(),
            Page = page,
            HasMore = rows.Count > 20,

            Insights =
                insights.Analyze(
                    await query
                        .Where(s =>
                            !s.IsImportedDuplicate)
                        .OrderByDescending(s =>
                            s.SleepDate)
                        .Take(90)
                        .ToListAsync())
        });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        int id = 0)
    {
        if (id != 0)
        {
            var row =
                await db.SleepLogs
                    .AsNoTracking()
                    .SingleOrDefaultAsync(s =>
                        s.Id == id &&
                        s.UserId == user.GetId());

            if (row == null)
                return NotFound();

            return View(new SleepForm
            {
                Id = row.Id,
                ActualBedtime = row.ActualBedtime,
                WakeTime = row.WakeTime,
                PlannedBedtime = row.PlannedBedtime,
                SleepRating = row.SleepRating,
                FallingAsleep = row.FallingAsleep,
                Awakenings = row.Awakenings,
                Notes = row.Notes
            });
        }

        var now = clock.Now;

        var preference =
            await preferences.GetAsync(
                user.GetId(),
                Request.Cookies["TargetSleepTime"]);

        var planned =
            PreferenceService.NextBedtime(
                now.Date.AddDays(-1),
                preference.PlannedBedtime);

        return View(new SleepForm
        {
            ActualBedtime = now.AddHours(-8),
            WakeTime = now,

            PlannedBedtime =
                planned <= now
                    ? planned
                    : planned.AddDays(-1)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(
        SleepForm form)
    {
        if (!ModelState.IsValid)
        {
            LocalizeErrors(text);
            return View(form);
        }

        var error =
            await service.SaveAsync(
                user.GetId(),
                form);

        if (error == "NotFound")
            return NotFound();

        if (error != null)
        {
            ModelState.AddModelError(
                "",
                text[error]);

            return View(form);
        }

        return RedirectToAction(
            nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(
        int id)
    {
        await db.SleepLogs
            .Where(s =>
                s.Id == id &&
                s.UserId == user.GetId())
            .ExecuteDeleteAsync();

        return RedirectToAction(
            nameof(Index));
    }
}