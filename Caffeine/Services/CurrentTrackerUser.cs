using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace Caffeine.Services;

public sealed class CurrentTrackerUser(IHttpContextAccessor accessor, IDataProtectionProvider protection)
{
    public const string CookieName = "CaffeineGuestV2";
    private readonly IDataProtector protector = protection.CreateProtector("Caffeine.Guest.v2");
    private string? cachedId;
    private HttpContext Context => accessor.HttpContext!;

    private static bool IsGuestId(string id) =>
        id.StartsWith("Guest_", StringComparison.Ordinal) &&
        Guid.TryParseExact(id[6..], "D", out var guid) &&
        guid != Guid.Empty;

    public string GetId()
    {
        if (cachedId != null)
            return cachedId;

        var accountId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Context.User.Identity?.IsAuthenticated == true && accountId != null)
            return cachedId = accountId;

        var existing = GetGuestId();
        var id = existing ?? "Guest_" + Guid.NewGuid().ToString("D");

        if (existing == null || !Context.Request.Cookies.ContainsKey(CookieName))
        {
            Context.Response.Cookies.Append(
                CookieName,
                protector.Protect(id),
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Context.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true,
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    Path = "/"
                });

            Context.Response.Cookies.Delete("GuestId");
        }

        return cachedId = id;
    }

    public string? GetGuestId()
    {
        var token = Context.Request.Cookies[CookieName];

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var id = protector.Unprotect(token);
                return IsGuestId(id) ? id : null;
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        // Backward compatibility: legacy random GUIDs are existing bearer credentials.
        // Strictly restrict to the guest namespace; numeric account IDs are NEVER accepted.
        // An invalid V2 token must not fall back to a legacy token.
        var legacy = Context.Request.Cookies["GuestId"];

        return legacy != null && IsGuestId(legacy)
            ? legacy
            : null;
    }

    public void ClearGuest()
    {
        Context.Response.Cookies.Delete(CookieName);
        Context.Response.Cookies.Delete("GuestId");
    }
}