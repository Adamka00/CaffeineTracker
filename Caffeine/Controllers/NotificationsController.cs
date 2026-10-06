using Microsoft.AspNetCore.Mvc;
namespace Caffeine.Controllers;
// Old links are explicit tombstones. No subscription writes, keys, sender or worker.
public sealed class NotificationsController : Controller
{
    [HttpGet] public IActionResult Index() => RedirectToAction("Index", "Help");
    [HttpPost] public IActionResult Index(string? unused) => StatusCode(410);
    [HttpPost] public IActionResult Subscribe() => StatusCode(410);
    [HttpPost] public IActionResult Unsubscribe() => StatusCode(410);
    [HttpPost] public IActionResult UnsubscribeAll() => StatusCode(410);
}
