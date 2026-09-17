using System.Globalization;
using Caffeine.Models;

namespace Caffeine.Services;

public static class BeverageDisplay
{
    private static readonly Dictionary<int, string> EnglishNames = new()
    {
        [1] = "Red Bull (Classic / Sugarfree)",
        [2] = "Red Bull Coconut-Blueberry (Edition)",
        [3] = "Monster Energy (Original)",
        [4] = "Monster Ultra (White/Zero)",
        [5] = "Monster Mango Loco",
        [6] = "Hell Energy Classic",
        [7] = "Hell Energy Zero",
        [8] = "Burn Original",
        [9] = "Bomba! Classic",
        [10] = "Hell Strong (Apple / Focus)",
        [11] = "Hell Strong Watermelon",
        [12] = "Reign Total Body Fuel",
        [13] = "Hell Ice Coffee Latte / Cappuccino",
        [14] = "Hell Ice Coffee Double Espresso",
        [15] = "Starbucks Frappuccino (bottled)",
        [16] = "Mizo Coffee (carton)",
        [17] = "Espresso (café)",
        [18] = "Double Espresso",
        [19] = "Long Coffee (Lungo)",
        [20] = "Cappuccino",
        [21] = "Filter Coffee (mug)",
        [22] = "Instant Coffee (Nescafé, mug)",
        [23] = "Nespresso (Original Espresso capsule)",
        [24] = "Nespresso (Original Lungo capsule)",
        [25] = "Dolce Gusto (Espresso capsule)",
        [26] = "Dolce Gusto (Lungo / Grande capsule)",
        [27] = "Coca-Cola (Classic / Zero)",
        [28] = "Pepsi",
        [29] = "Pepsi Max",
        [30] = "Dr Pepper",
        [31] = "Black Tea (mug)",
        [32] = "Green Tea (mug)",
        [33] = "Yerba Mate",
        [34] = "Matcha (traditional preparation)",
        [35] = "Mountain Dew",
        [36] = "Dolce Gusto (Iced Frappé)",
        [37] = "Dolce Gusto (Flat White)",
        [38] = "Dolce Gusto (Starbucks Caramel Macchiato)",
    };

    public static string Name(Beverage beverage) => beverage.Category != "Custom" &&
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en" &&
        EnglishNames.TryGetValue(beverage.Id, out var name) ? name : beverage.Name;

    public static string EvidenceKey(Beverage beverage) => beverage.Category == "Custom" ? "UserProvided" :
        beverage.Id is 1 or 2 or 3 or 4 or 6 or 7 or 10 or 11 ? "ManufacturerValue" :
        beverage.Category is "Coffee" or "Tea" or "Capsule" ? "PreparationEstimate" : "CheckLabel";
}