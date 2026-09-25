using System.Globalization;
using Caffeine.Services;
using Caffeine.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

namespace Caffeine.Controllers;

public partial class AccountController
{
    [HttpGet]
    [ResponseCache(
        NoStore = true,
        Location = ResponseCacheLocation.None)]
    public IActionResult ForgotPassword() =>
        View(new ForgotPasswordForm());


    [HttpPost]
    [EnableRateLimiting("password-reset")]
    [ResponseCache(
        NoStore = true,
        Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordForm model,
        [FromServices] PasswordResetService service,
        [FromServices] IStringLocalizer<SharedResource> text)
    {
        if (!ModelState.IsValid)
        {
            LocalizeErrors(text);
            return View(model);
        }

        var timer =
            System.Diagnostics.Stopwatch.StartNew();

        await service.RequestAsync(
            model.Email,
            CultureInfo
                .CurrentUICulture
                .TwoLetterISOLanguageName);

        // SMTP runs outside the request.
        // A floor reduces timing differences for unknown accounts.
        var remaining =
            TimeSpan.FromMilliseconds(300)
            - timer.Elapsed;

        if (remaining > TimeSpan.Zero)
        {
            await Task.Delay(
                remaining,
                HttpContext.RequestAborted);
        }

        ModelState.Clear();

        return View(
            new ForgotPasswordForm
            {
                Submitted = true
            });
    }


    [HttpGet]
    [ResponseCache(
        NoStore = true,
        Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> ResetPassword(
        string? token,
        [FromServices] PasswordResetService service)
    {
        Response.Headers["Referrer-Policy"] =
            "no-referrer";

        return View(
            new ResetPasswordForm
            {
                Token = token ?? "",

                InvalidToken =
                    !await service
                        .IsValidAsync(token)
            });
    }


    [HttpPost]
    [EnableRateLimiting("password-reset")]
    [ResponseCache(
        NoStore = true,
        Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordForm model,
        [FromServices] PasswordResetService service,
        [FromServices] IStringLocalizer<SharedResource> text)
    {
        Response.Headers["Referrer-Policy"] =
            "no-referrer";

        if (!ModelState.IsValid)
        {
            LocalizeErrors(text);
            return View(model);
        }

        var success =
            await service.ResetAsync(
                model.Token,
                model.Password);

        ModelState.Clear();

        return View(
            new ResetPasswordForm
            {
                Completed = success,
                InvalidToken = !success
            });
    }
}