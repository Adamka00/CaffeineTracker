using System.ComponentModel.DataAnnotations;
using Caffeine.Services;

namespace Caffeine.ViewModels;
public sealed class OwnDrinkForm
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = "";

    [Microsoft.AspNetCore.Mvc.ModelBinder(BinderType = typeof(CaffeineAmountBinder)), Range(0, 1000)]
    public double? CaffeinePer100Ml { get; set; }

    [Range(1, 2000)]
    public int AmountMl { get; set; } = 250;
    public string Category { get; set; } = "Custom";
}
