using Caffeine.Models;
using System;
using System.Collections.Generic;

namespace Caffeine.Services
{
    public interface ICaffeineCalculatorService
    {

        double GetCurrentTotalActiveCaffeine(IEnumerable<CaffeineLog> dailyLogs, DateTime currentTime);


        DateTime? EstimateSleepReadiness(IEnumerable<CaffeineLog> dailyLogs, DateTime currentTime, double thresholdMg = 25.0);
    }
}