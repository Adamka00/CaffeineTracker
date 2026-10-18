namespace Caffeine.ViewModels;
public sealed class EmailSettingsForm
{
    public bool Announcements { get; set; }
    public bool Weekly { get; set; }

    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
    public bool Confirmed { get; set; }
}

public sealed record EmailTokenView(string Token, bool Valid, bool Complete = false);
