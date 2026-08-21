namespace Caffeine.Models
{
    public class Beverage
    {
        public int Id { get; set; }
        
        // Pl. "Red Bull Kókusz-Áfonya"
        public string Name { get; set; } = string.Empty;
        
        // Kategória ikon/szín miatt (EnergyDrink, Coffee, Tea, stb.)
        public string Category { get; set; } = string.Empty;
        
        // Hány mg koffein van 100 ml-ben (ebből könnyű számolni bármilyen adagot)
        public double CaffeinePer100Ml { get; set; }
        
        // Alapértelmezett adag (pl. egy doboz RedBull 250ml)
        public int DefaultPortionMl { get; set; }
    }
}