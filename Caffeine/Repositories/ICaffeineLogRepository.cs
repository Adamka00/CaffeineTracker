using Caffeine.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Caffeine.Repositories
{
    public interface ICaffeineLogRepository
    {
        Task AddLogAsync(CaffeineLog log);
        Task<IEnumerable<CaffeineLog>> GetLogsForDateAsync(DateTime date);
        Task<IEnumerable<CaffeineLog>> GetRecentLogsAsync(int count);
        Task DeleteLogAsync(int id);
        
        Task<IEnumerable<CaffeineLog>> GetLogsSinceAsync(DateTime since);
    }
}