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

        public DateTime? EstimateSleepReadiness(IEnumerable<CaffeineLog> logs, DateTime now, double threshold)
        {
            if (!logs.Any()) return null;

            // 1. Keresd meg a legutolsó ital idejét, és add hozzá a 45 perc felszívódást!
            var lastDrinkTime = logs.Max(l => l.ConsumedAt);
            var peakTime = lastDrinkTime.AddMinutes(45);

            // 2. A keresést mindig a "most" ÉS a "legutóbbi csúcs" közül a KÉSŐBBITŐL indítjuk!
            // Így nem veri át a rendszert az, hogy épp most iszod és még nem szívódott fel.
            var searchStartTime = now > peakTime ? now : peakTime;

            // 3. Ha a csúcson is a küszöb alatt vagyunk, akkor azonnal mehetünk aludni
            if (GetCurrentTotalActiveCaffeine(logs, searchStartTime) <= threshold)
            {
                return null;
            }

            // 4. Pörgetjük az időt előre, amíg be nem esik a küszöb alá
            var time = searchStartTime;
            while (GetCurrentTotalActiveCaffeine(logs, time) > threshold)
            {
                time = time.AddMinutes(5); // 5 perces lépésköz a pontosságért
        
                // Biztonsági fék, hogy ne pörögjön végtelen ciklusba extrém túladagolás esetén
                if (time > now.AddDays(2)) break; 
            }

            return time;
        }
    }
}