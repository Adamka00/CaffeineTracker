using Caffeine.Data;
using Caffeine.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;
public sealed class OwnDrinkService(AppDbContext db)
{
    public static readonly string[] Categories = ["Energy Drink", "Coffee", "Tea", "Soda", "Custom"];
    public async Task<string?> UpdateAsync(string owner, OwnDrinkForm form)
    {
        if (string.IsNullOrWhiteSpace(form.Name) || form.Name.Length > 120 || form.CaffeinePer100Ml is not { } mg || !double.IsFinite(mg) || mg < 0 || mg > 1000 || form.AmountMl < 1 || form.AmountMl > 2000 || !Categories.Contains(form.Category))
            return "InvalidCustomDrink";
        var drink = await db.Beverages.SingleOrDefaultAsync(b => b.Id == form.Id && b.OwnerId == owner && !b.IsArchived);
        if (drink == null)
            return "NotFound";
        drink.Name = form.Name.Trim();
        drink.CaffeinePer100Ml = mg;
        drink.DefaultPortionMl = form.AmountMl;
        drink.Category = form.Category;
        await db.SaveChangesAsync();
        return null;
    }

    public async Task<bool> ArchiveAsync(string owner, int id)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var count = await db.Beverages.Where(b => b.Id == id && b.OwnerId == owner).ExecuteUpdateAsync(s => s.SetProperty(b => b.IsArchived, true));
        if (count == 0)
            return false;
        await db.FavoriteDrinks.Where(f => f.UserId == owner && f.BeverageId == id).ExecuteDeleteAsync();
        await tx.CommitAsync();
        return true;
    }
}
