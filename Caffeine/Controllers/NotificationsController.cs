using System.Globalization;
using Caffeine.Data;
using Caffeine.Models;
using Caffeine.Services;
using Caffeine.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Caffeine.Controllers;

public sealed class NotificationsController(
    AppDbContext db,
    CurrentTrackerUser user,
    PushConfiguration config,
    PushSubscriptionService subscriptions,
    IStringLocalizer<SharedResource> text)
    : FeatureController
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var p = await db.NotificationPreferences
                    .AsNoTracking()
                    .SingleOrDefaultAsync(p => p.UserId == user.GetId())
                ?? new NotificationPreference
                {
                    StreakReminder = true,
                    SleepCheckIn = true,
                    CutoffReminder = true,
                    ThresholdReached = true
                };

        return View(await Populate(new()
        {
            StreakReminder = p.StreakReminder,
            SleepCheckIn = p.SleepCheckIn,
            CutoffReminder = p.CutoffReminder,
            ThresholdReached = p.ThresholdReached,
            StreakTime = p.StreakTime,
            MorningTime = p.MorningTime,
            CutoffDoseMg = p.CutoffDoseMg
        }));
    }


    private async Task<NotificationForm> Populate(
        NotificationForm form)
    {
        form.PushConfigured =
            config.IsConfigured;


        form.PublicKey =
            config.IsConfigured
                ? config.PublicKey
                : "";


        form.DeviceCount =
            await db.BrowserPushSubscriptions
                .CountAsync(
                    s =>
                        s.UserId ==
                        user.GetId());


        return form;
    }


    [HttpPost]
    public async Task<IActionResult> Index(
        NotificationForm form)
    {
        if (!PreferenceService.ValidTime(
                form.StreakTime)
            ||
            !PreferenceService.ValidTime(
                form.MorningTime))
        {
            ModelState.AddModelError(
                "",
                text["InvalidInput"]);
        }


        if (!ModelState.IsValid)
        {
            LocalizeErrors(text);

            return View(
                await Populate(form));
        }


        var id =
            user.GetId();


        var culture =
            CultureInfo.CurrentUICulture
                .TwoLetterISOLanguageName ==
            "en"
                ? "en"
                : "hu";


        await db.Database
            .ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO NotificationPreferences
                    (
                        UserId,
                        StreakReminder,
                        SleepCheckIn,
                        CutoffReminder,
                        ThresholdReached,
                        StreakTime,
                        MorningTime,
                        CutoffDoseMg,
                        Culture
                    )
                VALUES
                    (
                        {id},
                        {form.StreakReminder},
                        {form.SleepCheckIn},
                        {form.CutoffReminder},
                        {form.ThresholdReached},
                        {form.StreakTime},
                        {form.MorningTime},
                        {form.CutoffDoseMg},
                        {culture}
                    )
                ON CONFLICT(UserId)
                DO UPDATE SET
                    StreakReminder =
                        excluded.StreakReminder,

                    SleepCheckIn =
                        excluded.SleepCheckIn,

                    CutoffReminder =
                        excluded.CutoffReminder,

                    ThresholdReached =
                        excluded.ThresholdReached,

                    StreakTime =
                        excluded.StreakTime,

                    MorningTime =
                        excluded.MorningTime,

                    CutoffDoseMg =
                        excluded.CutoffDoseMg,

                    Culture =
                        excluded.Culture;
                """);


        TempData["NotificationSaved"] =
            true;


        return RedirectToAction(
            nameof(Index));
    }


    [HttpPost]
    [EnableRateLimiting(
        "push-subscription")]
    public async Task<IActionResult> Subscribe(
        PushForm form)
    {
        if (!config.IsConfigured)
            return StatusCode(503);


        if (!ModelState.IsValid ||
            !PushSubscriptionService.Valid(
                form))
        {
            return BadRequest();
        }


        if (!await subscriptions
                .SubscribeAsync(
                    user.GetId(),
                    form))
        {
            return Conflict();
        }


        Response.Cookies.Append(
            "KoffiPushDevice",

            PushSubscriptionService
                .HashEndpoint(
                    form.Endpoint),

            new CookieOptions
            {
                HttpOnly = true,

                Secure =
                    Request.IsHttps,

                SameSite =
                    SameSiteMode.Strict,

                IsEssential = true,

                MaxAge =
                    TimeSpan.FromDays(365)
            });


        return Ok();
    }


    [HttpPost]
    public async Task<IActionResult> Unsubscribe(
        string endpoint)
    {
        if (string.IsNullOrWhiteSpace(
                endpoint)
            ||
            endpoint.Length > 2048)
        {
            return BadRequest();
        }


        await subscriptions
            .UnsubscribeAsync(
                user.GetId(),
                endpoint);


        Response.Cookies.Delete(
            "KoffiPushDevice");


        return Ok();
    }


    [HttpPost]
    public async Task<IActionResult> UnsubscribeAll()
    {
        await db.BrowserPushSubscriptions
            .Where(
                s =>
                    s.UserId ==
                    user.GetId())
            .ExecuteDeleteAsync();


        Response.Cookies.Delete(
            "KoffiPushDevice");


        return RedirectToAction(
            nameof(Index));
    }
}