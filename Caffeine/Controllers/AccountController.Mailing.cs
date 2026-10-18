using System.Globalization;
using System.Security.Claims;
using Caffeine.Services;
using Caffeine.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Controllers;
public partial class AccountController
{
    private int AccountId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [HttpGet, Authorize]
    public async Task<IActionResult> EmailSettings()
    {
        var p = await _context.MailingPreferences.AsNoTracking().SingleOrDefaultAsync(p => p.UserId == AccountId);
        var confirmed = await _context.Users.Where(u => u.Id == AccountId).Select(u => u.EmailConfirmed).SingleAsync();
        return View(new EmailSettingsForm { Announcements = p?.Announcements ?? false, Weekly = p?.Weekly ?? false, Confirmed = confirmed });
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> EmailSettings(EmailSettingsForm form, [FromServices] MailingPreferenceService preferences)
    {
        form.Confirmed = await _context.Users.Where(u => u.Id == AccountId).Select(u => u.EmailConfirmed).SingleAsync();
        if (!ModelState.IsValid || !await preferences.SaveAsync(AccountId, form.Announcements, form.Weekly, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName))
        {
            ModelState.AddModelError("", _text["VerifyBeforeSubscribe"]);
            return View(form);
        }

        TempData["EmailNotice"] = "MailPreferencesSaved";
        return RedirectToAction(nameof(EmailSettings));
    }

    [HttpPost, Authorize, EnableRateLimiting("password-reset")]
    public async Task<IActionResult> ResendConfirmation([FromServices] EmailConfirmationService confirmation)
    {
        TempData["EmailNotice"] = await confirmation.RequestAsync(AccountId, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
        return RedirectToAction(nameof(EmailSettings));
    }

    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> ConfirmEmail(string? token, [FromServices] EmailConfirmationService confirmation)
    {
        Response.Headers["Referrer-Policy"] = "no-referrer";
        return View(new EmailTokenView(token ?? "", await confirmation.ValidAsync(token)));
    }

    [HttpPost, ActionName("ConfirmEmail"), EnableRateLimiting("password-reset"), ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> ConfirmEmailPost(string? token, [FromServices] EmailConfirmationService confirmation)
    {
        Response.Headers["Referrer-Policy"] = "no-referrer";
        var ok = await confirmation.ConfirmAsync(token);
        return View(new EmailTokenView("", ok, ok));
    }

    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Unsubscribe(string? token, [FromServices] MailingPreferenceService preferences)
    {
        Response.Headers["Referrer-Policy"] = "no-referrer";
        return View(new EmailTokenView(token ?? "", preferences.Read(token) != null));
    }

    [HttpPost, ActionName("Unsubscribe"), EnableRateLimiting("password-reset"), ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> UnsubscribePost(string token, [FromServices] MailingPreferenceService preferences)
    {
        Response.Headers["Referrer-Policy"] = "no-referrer";
        var ok = await preferences.UnsubscribeAsync(token);
        return View(new EmailTokenView("", ok, ok));
    }
}
