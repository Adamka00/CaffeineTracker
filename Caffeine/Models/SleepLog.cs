using System.ComponentModel.DataAnnotations.Schema;

namespace Caffeine.Models;

public sealed class SleepLog
{
    public int Id { get; set; }
    public bool IsImportedDuplicate { get; set; }
    public string UserId { get; set; } = "";

    // Date of waking, in the existing tracker timezone.
    public DateTime SleepDate { get; set; }

    public DateTime? PlannedBedtime { get; set; }
    public DateTime ActualBedtime { get; set; }
    public DateTime WakeTime { get; set; }

    public int SleepRating { get; set; }
    public int? FallingAsleep { get; set; }
    public int? Awakenings { get; set; }

    public double EstimatedCaffeineAtBedtime { get; set; }
    public double PreviousDayCaffeineMg { get; set; }
    public DateTime? LastCaffeineAt { get; set; }

    public string? Notes { get; set; }

    [NotMapped]
    public double DurationHours => (WakeTime - ActualBedtime).TotalHours;
}