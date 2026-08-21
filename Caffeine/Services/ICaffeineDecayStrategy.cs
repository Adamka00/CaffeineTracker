using System;

namespace Caffeine.Services
{
    public interface ICaffeineDecayStrategy
    {
        // Kiszámolja, mennyi aktív koffein van a szervezetben egy adott fogyasztásból
        double CalculateActiveCaffeine(double initialAmountMg, DateTime consumedAt, DateTime currentTime);
    }
}