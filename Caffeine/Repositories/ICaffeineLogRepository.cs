using Caffeine.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Caffeine.Repositories
{
    public interface ICaffeineLogRepository
    {
        Task<IEnumerable<CaffeineLog>> GetLogsForDateAsync(DateTime date, string userId);
        Task<IEnumerable<CaffeineLog>> GetLogsSinceAsync(DateTime since, string userId);
        Task AddLogAsync(CaffeineLog log);
        Task DeleteLogAsync(int id, string userId);
        Task TransferLogsAsync(string oldUserId, string newUserId);
        Task DeleteAllLogsForUserAsync(string userId);
    }
}