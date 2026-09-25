namespace Caffeine.Models;

public sealed class TrackerPreference
{
    public string UserId { get; set; } = "";
    public string PlannedBedtime { get; set; } = "23:00";
    public double TargetMg { get; set; } = 25;
}