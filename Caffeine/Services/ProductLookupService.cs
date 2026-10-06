using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Caffeine.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
namespace Caffeine.Services;

public sealed record ProductSuggestion(string? Name, int? AmountMl, double? CaffeinePer100Ml,
    string Source, string Barcode, int? BeverageId = null);
public sealed record ProductLookupResult(string Status, ProductSuggestion? Product = null);
public interface IOpenFoodFactsClient
{
    Task<ProductLookupResult> FindAsync(string barcode, string culture, CancellationToken token);
}
public sealed class OpenFoodFactsClient(HttpClient client) : IOpenFoodFactsClient
{
    public async Task<ProductLookupResult> FindAsync(string barcode, string culture, CancellationToken token)
    {
        if (!ProductLookupService.ValidBarcode(barcode)) return new("invalid");
        try {
            using var response = await client.GetAsync("api/v3/product/" + barcode +
                "?fields=product_name,product_name_hu,product_name_en,product_name_de,quantity,product_quantity,product_quantity_unit,nutriments,nutrition_data_per", HttpCompletionOption.ResponseHeadersRead, token);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return new("not-found");
            if (!response.IsSuccessStatusCode) return new("unavailable");
            if (response.Content.Headers.ContentLength > 131072) return new("unavailable");
            await using var stream = await response.Content.ReadAsStreamAsync(token);
            using var buffer = new MemoryStream();
            var bytes = new byte[4096]; int read;
            while ((read = await stream.ReadAsync(bytes, token)) > 0) {
                if (buffer.Length + read > 131072) return new("unavailable");
                buffer.Write(bytes, 0, read);
            }
            using var json = JsonDocument.Parse(buffer.ToArray());
            if (!json.RootElement.TryGetProperty("product", out var product) || product.ValueKind != JsonValueKind.Object) return new("not-found");
            return new("found", Map(product, barcode, culture));
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested) { return new("unavailable"); }
        catch (HttpRequestException) { return new("unavailable"); }
        catch (JsonException) { return new("unavailable"); }
        catch (IOException) { return new("unavailable"); }
    }
    private static string? Text(JsonElement e, string key) => e.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
    private static double? Number(JsonElement e, string key) {
        if (!e.TryGetProperty(key, out var v)) return null;
        return double.TryParse(v.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var n) && double.IsFinite(n) ? n : null;
    }
    public static ProductSuggestion Map(JsonElement product, string barcode, string culture)
    {
        culture = culture is "hu" or "de" ? culture : "en";
        var name = Text(product, "product_name_" + culture) ?? Text(product, "product_name") ?? Text(product, "product_name_en");
        if (string.IsNullOrWhiteSpace(name)) name = null;
        else name = name.Trim()[..Math.Min(name.Trim().Length, 120)];
        var quantity = Number(product, "product_quantity");
        var unit = Text(product, "product_quantity_unit");
        double? ml = unit switch { "ml" => quantity, "cl" => quantity * 10, "l" => quantity * 1000, _ => null };
        if (ml == null) {
            var m = Regex.Match(Text(product, "quantity") ?? "", @"^\s*(\d+(?:[.,]\d+)?)\s*(ml|cl|l)\s*$", RegexOptions.IgnoreCase);
            if (m.Success && double.TryParse(m.Groups[1].Value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var q))
                ml = q * (m.Groups[2].Value.ToLowerInvariant() switch { "l" => 1000, "cl" => 10, _ => 1 });
        }
        int? amount = ml is >= 1 and <= 2000 && Math.Abs(ml.Value - Math.Round(ml.Value)) < 0.001 ? (int)Math.Round(ml.Value) : null;
        double? caffeine = null;
        if (product.TryGetProperty("nutriments", out var nutrients) && nutrients.ValueKind == JsonValueKind.Object) {
            // OFF standardizes caffeine to grams. Do not guess density or parse ingredient text.
            var perMl = Number(nutrients, "caffeine_100ml");
            var per100 = perMl ?? (Text(product, "nutrition_data_per") == "100ml" ? Number(nutrients, "caffeine_100g") : null);
            if (per100.HasValue) caffeine = per100.Value * 1000;
        }
        if (caffeine is < 0 or > 1000 || (caffeine.HasValue && !double.IsFinite(caffeine.Value))) caffeine = null;
        return new(name, amount, caffeine, "OpenFoodFacts", barcode);
    }
}
public sealed class ProductLookupService(AppDbContext db, IOpenFoodFactsClient external, IMemoryCache cache, IConfiguration config)
{
    public static bool ValidBarcode(string? code) {
        if (code == null || code.Length is not (8 or 12 or 13 or 14) || !code.All(char.IsAsciiDigit)) return false;
        var sum = 0;
        for (var i = code.Length - 2; i >= 0; i--) sum += (code[i] - '0') * ((code.Length - 2 - i) % 2 == 0 ? 3 : 1);
        return (10 - sum % 10) % 10 == code[^1] - '0';
    }
    public async Task<ProductLookupResult> FindAsync(string user, string code, string culture, CancellationToken token = default) {
        if (!ValidBarcode(code)) return new("invalid");
        var local = await db.BeverageBarcodes.AsNoTracking().Include(p => p.Beverage)
            .SingleOrDefaultAsync(p => p.Barcode == code && p.Beverage.OwnerId == null && p.Beverage.Category != "Custom", token);
        if (local != null) return new("found", new(BeverageDisplay.Name(local.Beverage), local.Beverage.DefaultPortionMl,
            local.Beverage.CaffeinePer100Ml, "Koffi", code, local.BeverageId));
        if (!config.GetValue("Products:OpenFoodFactsEnabled", true)) return new("unavailable");
        var key = "product-v4:" + code + ":" + culture;
        if (cache.TryGetValue<ProductLookupResult>(key, out var cached)) return cached!;
        var result = await external.FindAsync(code, culture, token);
        if (result.Status is "found" or "not-found") cache.Set(key, result, TimeSpan.FromHours(result.Status == "found" ? 12 : 1));
        return result;
    }
}
