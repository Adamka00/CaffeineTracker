using Caffeine.Data;
using Caffeine.Models;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;

public sealed class DrinkCatalog(AppDbContext db)
{
    public IQueryable<Beverage> ForUser(string userId) =>
        db.Beverages
            .AsNoTracking()
            .Where(b =>
                (b.OwnerId == null && b.Category != "Custom") ||
                b.OwnerId == userId);

    public async Task<List<Beverage>> ListAsync(string userId) =>
        (await ForUser(userId).ToListAsync())
        .OrderBy(BeverageDisplay.Name)
        .ToList();
}