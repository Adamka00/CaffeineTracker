using System.ComponentModel.DataAnnotations;
using Caffeine.Services;
namespace Caffeine.ViewModels;
public sealed class BarcodeForm {
    [Required, StringLength(14, MinimumLength = 8)] public string Code { get; set; } = "";
}
public sealed record BarcodeReview(ProductLookupResult Result, DateTime ConsumedAt);
