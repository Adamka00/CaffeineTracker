using Caffeine.Data;
using Caffeine.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caffeine.Repositories;

namespace CaffeineTracker.Repositories
{
    public class CaffeineLogRepository : ICaffeineLogRepository
    {
        private readonly AppDbContext _context;

        public CaffeineLogRepository(AppDbContext context)
        {
            _context = context;
        }

        // 1. Új napló hozzáadása
        public async Task AddLogAsync(CaffeineLog log)
        {
            await _context.CaffeineLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        // 2. EZ HIÁNYZOTT: Lekérdezi egy adott naptári nap adatait (éjféltől éjfélig)
        public async Task<IEnumerable<CaffeineLog>> GetLogsForDateAsync(DateTime date)
        {
            var startOfDay = date.Date; // A nap kezdete (00:00:00)
            var endOfDay = startOfDay.AddDays(1); // A következő nap kezdete

            return await _context.CaffeineLogs
                .Include(l => l.Beverage)
                .Where(l => l.ConsumedAt >= startOfDay && l.ConsumedAt < endOfDay)
                .OrderBy(l => l.ConsumedAt)
                .ToListAsync();
        }

        // 3. Junnie javítása: Visszamenőleges lekérdezés (pl. az elmúlt 24 óra) a pontos kalkulációhoz
        public async Task<IEnumerable<CaffeineLog>> GetLogsSinceAsync(DateTime since)
        {
            return await _context.CaffeineLogs
                .Include(l => l.Beverage)
                .Where(l => l.ConsumedAt >= since)
                .OrderBy(l => l.ConsumedAt)
                .ToListAsync();
        }

        // 4. Legutóbbi X darab napló lekérdezése
        public async Task<IEnumerable<CaffeineLog>> GetRecentLogsAsync(int count)
        {
            return await _context.CaffeineLogs
                .Include(l => l.Beverage)
                .OrderByDescending(l => l.ConsumedAt)
                .Take(count)
                .ToListAsync();
        }

        // 5. Törlés
        public async Task DeleteLogAsync(int id)
        {
            var log = await _context.CaffeineLogs.FindAsync(id);
            if (log != null)
            {
                _context.CaffeineLogs.Remove(log);
                await _context.SaveChangesAsync();
            }
        }
    }
}