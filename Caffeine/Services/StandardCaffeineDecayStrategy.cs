using System;

namespace Caffeine.Services
{
    public class StandardCaffeineDecayStrategy : ICaffeineDecayStrategy
    {
        private const double PeakTimeMinutes = 45.0;
        private const double HalfLifeHours = 5.0;

        public double CalculateActiveCaffeine(double initialAmountMg, DateTime consumedAt, DateTime currentTime)
        {
            var timePassed = currentTime - consumedAt;

            if (timePassed.TotalMinutes <= 0) return 0;

            if (timePassed.TotalMinutes < PeakTimeMinutes)
            {
                return initialAmountMg * (timePassed.TotalMinutes / PeakTimeMinutes);
            }

            double hoursSincePeak = (timePassed.TotalMinutes - PeakTimeMinutes) / 60.0;
            return initialAmountMg * Math.Pow(0.5, hoursSincePeak / HalfLifeHours);
        }
    }
}