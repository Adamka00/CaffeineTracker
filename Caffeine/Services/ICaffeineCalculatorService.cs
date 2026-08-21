using Caffeine.Models;
using System;
using System.Collections.Generic;

namespace Caffeine.Services
{
    public interface ICaffeineCalculatorService
    {
        // Összesíti az aktuális pillanatnyi koffeinszintet
        double GetCurrentTotalActiveCaffeine(IEnumerable<CaffeineLog> dailyLogs, DateTime currentTime);
        
        // Kiszámolja, mikor esik a szint az alvásküszöb alá (pl. 25 mg)
        DateTime? EstimateSleepReadiness(IEnumerable<CaffeineLog> dailyLogs, DateTime currentTime, double thresholdMg = 25.0);
    }
}