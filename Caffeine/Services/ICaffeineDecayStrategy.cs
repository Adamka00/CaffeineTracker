using System;

namespace Caffeine.Services
{
    public interface ICaffeineDecayStrategy
    {

        double CalculateActiveCaffeine(double initialAmountMg, DateTime consumedAt, DateTime currentTime);
    }
}