using System.Globalization;
using Caffeine.Services;
using Caffeine.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace Caffeine.Controllers;
public sealed class BarcodeController(ProductLookupService products, CurrentTrackerUser user, TrackerClock clock) : Controller
{
    [HttpGet] public IActionResult Index() => View(new BarcodeForm());
    [HttpPost, EnableRateLimiting("product-lookup")]
    public async Task<IActionResult> Lookup(BarcodeForm form) {
        if (!ModelState.IsValid || !ProductLookupService.ValidBarcode(form.Code)) {
            ModelState.Clear(); ModelState.AddModelError("Code", "");
            ViewData["InvalidBarcode"] = true;
            return View("Index", form);
        }
        var result = await products.FindAsync(user.GetId(), form.Code, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, HttpContext.RequestAborted);
        return View("Review", new BarcodeReview(result, clock.Now));
    }
}
