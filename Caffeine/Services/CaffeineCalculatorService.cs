using Caffeine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Caffeine.Services;

namespace Caffeine.Services
{
    public class CaffeineCalculatorService : ICaffeineCalculatorService
    {
        private readonly ICaffeineDecayStrategy _decayStrategy;

        // Dependency Injection: Befecskendezzük a stratégiát
        public CaffeineCalculatorService(ICaffeineDecayStrategy decayStrategy)
        {
            _decayStrategy = decayStrategy;
        }

        public double GetCurrentTotalActiveCaffeine(IEnumerable<CaffeineLog> dailyLogs, DateTime currentTime)
        {
            if (dailyLogs == null || !dailyLogs.Any()) return 0;

            double totalActive = 0;
            foreach (var log in dailyLogs)
            {
                totalActive += _decayStrategy.CalculateActiveCaffeine(log.TotalCaffeineMg, log.ConsumedAt, currentTime);
            }

            return totalActive;
        }

        public DateTime? EstimateSleepReadiness(IEnumerable<CaffeineLog> dailyLogs, DateTime currentTime, double thresholdMg = 25.0)
        {
            if (dailyLogs == null || !dailyLogs.Any()) return currentTime;

            var currentLevel = GetCurrentTotalActiveCaffeine(dailyLogs, currentTime);
            if (currentLevel <= thresholdMg) return currentTime;

            // Szimuláció: Elindulunk a jelenből, és 15 percenként lépkedünk előre, amíg le nem esik a szint.
            // (Ez a legtisztább módszer többszöri fogyasztás szuperpozíciója esetén)
            var simulatedTime = currentTime;
            while (currentLevel > thresholdMg && simulatedTime < currentTime.AddHours(24))
            {
                simulatedTime = simulatedTime.AddMinutes(15);
                currentLevel = GetCurrentTotalActiveCaffeine(dailyLogs, simulatedTime);
            }

            return simulatedTime;
        }
    }
}