namespace Caffeine.Models;

// Separate from biological settings; additive table for existing databases.
public sealed class ExperiencePreference
{
    public string UserId { get; set; } = "";
    public bool TutorialCompleted { get; set; }
    public string LastSeenRelease { get; set; } = "";
    public DateTime? MorningDismissedDate { get; set; }
}

public sealed class BeverageBarcode
{
    public string Barcode { get; set; } = "";
    public int BeverageId { get; set; }
    public Beverage Beverage { get; set; } = null!;
}