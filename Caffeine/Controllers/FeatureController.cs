using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Caffeine.Controllers;

public abstract class FeatureController : Controller
{
    protected void LocalizeErrors(IStringLocalizer<SharedResource> text)
    {
        foreach (var entry in ModelState.Values.Where(v => v.Errors.Count > 0))
        {
            entry.Errors.Clear();
            entry.Errors.Add(text["InvalidInput"]);
        }
    }
}