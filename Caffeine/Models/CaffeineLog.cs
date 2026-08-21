using System;

namespace Caffeine.Models
{
    public class CaffeineLog
    {
        public int Id { get; set; }
        
        // Mikor itta? (UTC-ben érdemes tárolni a szerveren, de most a lokális idő is jó a demóhoz)
        public DateTime ConsumedAt { get; set; }
        
        // Mennyit ivott pontosan? (Ha nem a defaultot választotta)
        public int ConsumedAmountMl { get; set; }
        
        // Kiszámolt koffein mg-ban (denormalizáljuk, hogy gyors legyen a statisztika lekérdezése, 
        // és ha az alap ital receptje változik, a régi naplózás ne romoljon el)
        public double TotalCaffeineMg { get; set; }

        // Navigációs tulajdonság
        public int BeverageId { get; set; }
        public Beverage Beverage { get; set; } = null!;
    }
}