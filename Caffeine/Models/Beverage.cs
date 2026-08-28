namespace Caffeine.Models
{
    public class Beverage
    {
        public int Id { get; set; }


        public string Name { get; set; } = string.Empty;


        public string Category { get; set; } = string.Empty;


        public double CaffeinePer100Ml { get; set; }


        public int DefaultPortionMl { get; set; }
    }
}