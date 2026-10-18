using Caffeine.Services;
using Microsoft.AspNetCore.Mvc;

namespace Caffeine.Controllers;
public sealed class GuestPromptController(GuestPromptService service, CurrentTrackerUser user) : Controller
{
    [HttpPost]
    public async Task<IActionResult> Dismiss()
    {
        await service.DismissAsync(user.GetId());
        return LocalRedirect("/");
    }
}
