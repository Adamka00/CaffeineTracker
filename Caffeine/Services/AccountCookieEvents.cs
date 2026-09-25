using System.Security.Claims;
using Caffeine.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;

public sealed class AccountCookieEvents(
    AppDbContext db)
    : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(
        CookieValidatePrincipalContext context)
    {
        var id =
            context.Principal?
                .FindFirstValue(
                    ClaimTypes.NameIdentifier);


        if (!int.TryParse(
                id,
                out var userId))
        {
            context.RejectPrincipal();
            return;
        }


        var stamp =
            await db.Users
                .Where(u =>
                    u.Id == userId)
                .Select(u =>
                    u.SecurityStamp)
                .SingleOrDefaultAsync();


        // Old cookies have no stamp; valid until
        // the first password reset changes
        // the empty legacy stamp.
        if (stamp == null ||
            stamp !=
            (
                context.Principal?
                    .FindFirstValue(
                        "SecurityStamp")
                ?? ""
            ))
        {
            context.RejectPrincipal();

            await context.HttpContext
                .SignOutAsync(
                    CookieAuthenticationDefaults
                        .AuthenticationScheme);
        }
    }
}