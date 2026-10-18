
namespace Caffeine.Models;

public sealed class EmailConfirmationToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string TokenHash { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? UsedAtUtc { get; set; }
}

public sealed class MailingPreference
{
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public bool Announcements { get; set; }
    public bool Weekly { get; set; }
    public string Culture { get; set; } = "hu";
    public string AnnouncementVersion { get; set; } = "";
    public string WeeklyVersion { get; set; } = "";
    public DateTime? AnnouncementConsentUtc { get; set; }
    public DateTime? WeeklyConsentUtc { get; set; }
}

public sealed class MailingConsent
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string Kind { get; set; } = "";
    public bool Enabled { get; set; }
    public DateTime AtUtc { get; set; }
    public string PolicyVersion { get; set; } = "5.0";
}

public sealed class MailCampaign
{
    public int Id { get; set; }
    public string Key { get; set; } = "";
    // Serialized validated HU/EN/DE plain-text content. HTML is constructed with escaping.
    public string ContentJson { get; set; } = "";
    public bool Started { get; set; }
    public DateTime? TestSentAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class MailJob
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string Kind { get; set; } = "";
    public string DeduplicationKey { get; set; } = "";
    // Verification URLs are DataProtection-encrypted; never persist raw bearer tokens.
    public string ProtectedPayload { get; set; } = "";
    public string Culture { get; set; } = "hu";
    public int? CampaignId { get; set; }
    public MailCampaign? Campaign { get; set; }
    public DateTime? WeekStart { get; set; }
    public string State { get; set; } = "Pending";
    public int Attempts { get; set; }
    public string? LeaseId { get; set; }
    public DateTime? LeaseUntilUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime NextAttemptUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
}

public sealed class GuestPromptPreference
{
    public string UserId { get; set; } = "";
    public DateTime? DismissedUntil { get; set; }
}
