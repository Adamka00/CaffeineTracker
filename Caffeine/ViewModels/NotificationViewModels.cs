using System.ComponentModel.DataAnnotations;

namespace Caffeine.ViewModels;

public sealed class NotificationForm
{
    public bool StreakReminder { get; set; }

    public bool SleepCheckIn { get; set; }

    public bool CutoffReminder { get; set; }

    public bool ThresholdReached { get; set; }


    [Required]
    public string StreakTime { get; set; } =
        "20:00";


    [Required]
    public string MorningTime { get; set; } =
        "08:00";


    [Range(1, 2000)]
    public int CutoffDoseMg { get; set; } =
        80;


    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
    public bool PushConfigured { get; set; }


    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
    public string PublicKey { get; set; } =
        "";


    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
    public int DeviceCount { get; set; }
}


public sealed class PushForm
{
    [Required, StringLength(2048)]
    public string Endpoint { get; set; } =
        "";


    [Required, StringLength(128)]
    public string P256dh { get; set; } =
        "";


    [Required, StringLength(64)]
    public string Auth { get; set; } =
        "";
}