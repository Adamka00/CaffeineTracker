using System;

namespace Caffeine.Services
{
    public interface ICaffeineDecayStrategy
    {
        TimeSpan TimeToPeak { get; }

        double CalculateActiveCaffeine(
            double initialAmountMg,
            DateTime consumedAt,
            DateTime currentTime);
    }
}