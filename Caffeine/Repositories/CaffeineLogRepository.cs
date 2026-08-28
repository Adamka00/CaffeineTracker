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

        public async Task<IEnumerable<CaffeineLog>> GetLogsForDateAsync(DateTime date, string userId)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            return await _context.CaffeineLogs
                .Include(l => l.Beverage)
                .Where(l => l.UserId == userId && l.ConsumedAt >= startOfDay && l.ConsumedAt <= endOfDay)
                .ToListAsync();
        }

        public async Task<IEnumerable<CaffeineLog>> GetLogsSinceAsync(DateTime since, string userId)
        {
            return await _context.CaffeineLogs
                .Include(l => l.Beverage)
                .Where(l => l.UserId == userId && l.ConsumedAt >= since)
                .ToListAsync();
        }

        public async Task AddLogAsync(CaffeineLog log)
        {
            _context.CaffeineLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLogAsync(int id, string userId)
        {
            var log = await _context.CaffeineLogs.FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);
            if (log != null)
            {
                _context.CaffeineLogs.Remove(log);
                await _context.SaveChangesAsync();
            }
        }

        public async Task TransferLogsAsync(string oldUserId, string newUserId)
        {
            var logs = await _context.CaffeineLogs.Where(l => l.UserId == oldUserId).ToListAsync();
            foreach (var log in logs)
            {
                log.UserId = newUserId;
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllLogsForUserAsync(string userId)
        {
            var logs = await _context.CaffeineLogs.Where(l => l.UserId == userId).ToListAsync();
            _context.CaffeineLogs.RemoveRange(logs);
            await _context.SaveChangesAsync();
        }
    }
}