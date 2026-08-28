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


            var lastDrinkTime = logs.Max(l => l.ConsumedAt);
            var peakTime = lastDrinkTime.AddMinutes(45);



            var searchStartTime = now > peakTime ? now : peakTime;


            if (GetCurrentTotalActiveCaffeine(logs, searchStartTime) <= threshold)
            {
                return null;
            }


            var time = searchStartTime;
            while (GetCurrentTotalActiveCaffeine(logs, time) > threshold)
            {
                time = time.AddMinutes(5);


                if (time > now.AddDays(2)) break;
            }

            return time;
        }
    }
}