using Caffeine.Services;
using Microsoft.AspNetCore.Mvc;

namespace Caffeine.Controllers;

public sealed class HelpController(
    ExperienceService experience,
    CurrentTrackerUser user) : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult WhatsNew() => View(ReleaseCatalog.All);

    [HttpGet]
    public IActionResult Support() => View();

    [HttpPost]
    public async Task<IActionResult> Acknowledge(
        string kind,
        string? version)
    {
        if (kind is not ("tutorial" or "release" or "morning") ||
            (kind == "release" &&
             !ReleaseCatalog.IsKnown(version ?? "")))
        {
            return BadRequest();
        }

        await experience.SaveAsync(
            user.GetId(),
            kind,
            version);

        return NoContent();
    }
}