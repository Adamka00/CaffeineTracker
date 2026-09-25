using Caffeine.Data;
using Caffeine.Services;
using Caffeine.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Caffeine.Controllers;

public sealed class PlanningController(
    AppDbContext db,
    CurrentTrackerUser user,
    TrackerClock clock,
    PreferenceService preferences,
    CaffeinePlanningService planning,
    DrinkCatalog catalog,
    IStringLocalizer<SharedResource> text)
    : FeatureController
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var p = await preferences.GetAsync(
            user.GetId(),
            Request.Cookies["TargetSleepTime"]);

        return View(new PlanningForm
        {
            Bedtime = PreferenceService.NextBedtime(
                clock.Now,
                p.PlannedBedtime),

            IntakeAt = clock.Now,
            TargetMg = (int)p.TargetMg,
            Beverages = await catalog.ListAsync(user.GetId())
        });
    }

    [HttpPost]
    public async Task<IActionResult> Index(PlanningForm model)
    {
        var now = clock.Now;

        model.Beverages =
            await catalog.ListAsync(user.GetId());

        if (model.IntakeAt < now.AddMinutes(-10) ||
            model.IntakeAt > now.AddDays(2) ||
            model.Bedtime <= now ||
            model.Bedtime > now.AddDays(3) ||
            model.IntakeAt > model.Bedtime)
        {
            ModelState.AddModelError(
                "",
                text["InvalidPlanningTime"]);
        }

        if (!ModelState.IsValid)
        {
            LocalizeErrors(text);
            return View(model);
        }

        var dose = (double)model.DoseMg;

        if (model.BeverageId.HasValue)
        {
            var beverage =
                model.Beverages.SingleOrDefault(
                    b => b.Id == model.BeverageId);

            if (beverage == null)
                return NotFound();

            dose = Math.Round(
                beverage.CaffeinePer100Ml *
                model.AmountMl / 100,
                1);
        }

        var logs = await db.CaffeineLogs
            .AsNoTracking()
            .Where(l =>
                l.UserId == user.GetId() &&
                l.ConsumedAt <= now)
            .ToListAsync();

        model.Result = planning.Calculate(
            logs,
            now,
            model.Bedtime!.Value,
            model.IntakeAt!.Value,
            dose,
            model.TargetMg);

        // No SaveChanges: simulations never create data.
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var p = await preferences.GetAsync(
            user.GetId(),
            Request.Cookies["TargetSleepTime"]);

        return View(new PreferenceForm
        {
            PlannedBedtime = p.PlannedBedtime,
            TargetMg = (int)p.TargetMg
        });
    }

    [HttpPost]
    public async Task<IActionResult> Settings(
        PreferenceForm model)
    {
        if (!PreferenceService.ValidTime(
                model.PlannedBedtime))
        {
            ModelState.AddModelError(
                "",
                text["InvalidInput"]);
        }

        if (!ModelState.IsValid)
        {
            LocalizeErrors(text);
            return View(model);
        }

        await preferences.SaveAsync(
            user.GetId(),
            model.PlannedBedtime,
            model.TargetMg);

        return RedirectToAction(nameof(Index));
    }
}