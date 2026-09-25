using System.ComponentModel.DataAnnotations;
using Caffeine.Models;
using Caffeine.Services;

namespace Caffeine.ViewModels;

public sealed class SleepForm
{
    public int Id { get; set; }

    [Required]
    public DateTime? ActualBedtime { get; set; }

    [Required]
    public DateTime? WakeTime { get; set; }

    public DateTime? PlannedBedtime { get; set; }

    [Range(1, 5)]
    public int SleepRating { get; set; } = 3;

    [Range(1, 5)]
    public int? FallingAsleep { get; set; }

    [Range(0, 50)]
    public int? Awakenings { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
}

public sealed class SleepIndex
{
    public List<SleepLog> Logs { get; set; } = [];
    public List<SleepInsight> Insights { get; set; } = [];
    public int Page { get; set; }
    public bool HasMore { get; set; }
}