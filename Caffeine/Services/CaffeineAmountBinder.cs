using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Caffeine.Services;

// HTML number inputs submit '.', even on a Hungarian-language page.
public sealed class CaffeineAmountBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext context)
    {
        var value = context.ValueProvider.GetValue(context.ModelName);
        if (value == ValueProviderResult.None) return Task.CompletedTask;

        context.ModelState.SetModelValue(context.ModelName, value);

        var raw = value.FirstValue;

        if (string.IsNullOrWhiteSpace(raw))
            context.Result = ModelBindingResult.Success(null);
        else if (double.TryParse(
                     raw.Replace(',', '.'),
                     NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                     CultureInfo.InvariantCulture,
                     out var amount) && double.IsFinite(amount))
            context.Result = ModelBindingResult.Success(amount);
        else
            context.ModelState.TryAddModelError(
                context.ModelName,
                CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "hu"
                    ? "Érvénytelen koffeinérték."
                    : "Invalid caffeine value.");

        return Task.CompletedTask;
    }
}