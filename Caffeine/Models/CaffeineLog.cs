using System;

namespace Caffeine.Models
{
    public class CaffeineLog
    {
        public int Id { get; set; }


        public DateTime ConsumedAt { get; set; }


        public int ConsumedAmountMl { get; set; }



        public double TotalCaffeineMg { get; set; }


        public int BeverageId { get; set; }
        public Beverage Beverage { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
    }
}