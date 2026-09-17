using Caffeine.Data;
using Caffeine.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caffeine.Repositories
{
    public class CaffeineLogRepository : ICaffeineLogRepository
    {
        private readonly AppDbContext _context;

        public CaffeineLogRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<CaffeineLog>> GetLogsForDateAsync(
            DateTime date,
            string userId)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _context.CaffeineLogs
                .Include(l => l.Beverage)
                .Where(l =>
                    l.UserId == userId &&
                    l.ConsumedAt >= startOfDay &&
                    l.ConsumedAt < endOfDay)
                .ToListAsync();
        }


        public async Task<IEnumerable<CaffeineLog>> GetLogsSinceAsync(
            DateTime since,
            string userId)
        {
            return await _context.CaffeineLogs
                .Include(l => l.Beverage)
                .Where(l =>
                    l.UserId == userId &&
                    l.ConsumedAt >= since)
                .ToListAsync();
        }


        public async Task AddLogAsync(CaffeineLog log)
        {
            _context.CaffeineLogs.Add(log);

            await _context.SaveChangesAsync();
        }


        public async Task DeleteLogAsync(
            int id,
            string userId)
        {
            var log =
                await _context.CaffeineLogs
                    .FirstOrDefaultAsync(l =>
                        l.Id == id &&
                        l.UserId == userId);

            if (log != null)
            {
                _context.CaffeineLogs.Remove(log);

                await _context.SaveChangesAsync();
            }
        }


        public async Task TransferLogsAsync(
            string oldUserId,
            string newUserId)
        {
            if (!oldUserId.StartsWith(
                    "Guest_",
                    StringComparison.Ordinal) ||
                !Guid.TryParseExact(
                    oldUserId[6..],
                    "D",
                    out _))
            {
                throw new ArgumentException(
                    "Only guest data can be transferred.",
                    nameof(oldUserId));
            }


            var logs =
                await _context.CaffeineLogs
                    .Where(l =>
                        l.UserId == oldUserId)
                    .ToListAsync();


            var drinks =
                await _context.Beverages
                    .Where(b =>
                        b.OwnerId == oldUserId)
                    .ToListAsync();


            foreach (var drink in drinks)
            {
                drink.OwnerId = newUserId;
            }


            var favorites =
                await _context.FavoriteDrinks
                    .Where(f =>
                        f.UserId == oldUserId)
                    .ToListAsync();


            var existing =
                await _context.FavoriteDrinks
                    .Where(f =>
                        f.UserId == newUserId)
                    .ToListAsync();


            foreach (var favorite in favorites)
            {
                if (existing.Any(f =>
                        f.BeverageId == favorite.BeverageId &&
                        f.AmountMl == favorite.AmountMl))
                {
                    _context.FavoriteDrinks.Remove(
                        favorite);
                }
                else
                {
                    favorite.UserId = newUserId;
                }
            }


            foreach (var log in logs)
            {
                log.UserId = newUserId;
            }


            await _context.SaveChangesAsync();
        }


        public async Task DeleteAllLogsForUserAsync(
            string userId)
        {
            _context.FavoriteDrinks.RemoveRange(
                await _context.FavoriteDrinks
                    .Where(f =>
                        f.UserId == userId)
                    .ToListAsync());


            _context.Beverages.RemoveRange(
                await _context.Beverages
                    .Where(b =>
                        b.OwnerId == userId)
                    .ToListAsync());


            var logs =
                await _context.CaffeineLogs
                    .Where(l =>
                        l.UserId == userId)
                    .ToListAsync();


            _context.CaffeineLogs.RemoveRange(logs);

            await _context.SaveChangesAsync();
        }
    }
}