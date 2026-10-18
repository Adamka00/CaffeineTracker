using Caffeine.Data;
using Caffeine.Services;
using Caffeine.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Caffeine.Controllers;
public sealed class DrinksController(AppDbContext db, CurrentTrackerUser user, OwnDrinkService service, IStringLocalizer<SharedResource> text) : FeatureController
{
    [HttpGet]
    public async Task<IActionResult> Index() => View(await db.Beverages.AsNoTracking().Where(b => b.OwnerId == user.GetId() && !b.IsArchived).OrderBy(b => b.Name).ToListAsync());
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var b = await db.Beverages.AsNoTracking().SingleOrDefaultAsync(b => b.Id == id && b.OwnerId == user.GetId() && !b.IsArchived);
        return b == null ? NotFound() : View(new OwnDrinkForm { Id = b.Id, Name = b.Name, CaffeinePer100Ml = b.CaffeinePer100Ml, AmountMl = b.DefaultPortionMl, Category = b.Category });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(OwnDrinkForm form)
    {
        if (!ModelState.IsValid)
        {
            LocalizeErrors(text);
            return View(form);
        }

        var error = await service.UpdateAsync(user.GetId(), form);
        if (error == "NotFound")
            return NotFound();
        if (error != null)
        {
            ModelState.AddModelError("", text[error]);
            return View(form);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id) => await service.ArchiveAsync(user.GetId(), id) ? RedirectToAction(nameof(Index)) : NotFound();
}
