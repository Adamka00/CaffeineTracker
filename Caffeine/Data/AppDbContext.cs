using Caffeine.Models;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Beverage> Beverages { get; set; }
        public DbSet<CaffeineLog> CaffeineLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Lista
            
            modelBuilder.Entity<Beverage>().HasData(
    // --- ENERGIAITALOK (KLASSZIKUS ~32mg/100ml) ---
    new Beverage { Id = 1, Name = "Red Bull (Classic / Sugarfree)", Category = "Energy Drink", CaffeinePer100Ml = 32, DefaultPortionMl = 250 },
    new Beverage { Id = 2, Name = "Red Bull Kókusz-Áfonya (Edition)", Category = "Energy Drink", CaffeinePer100Ml = 32, DefaultPortionMl = 250 },
    new Beverage { Id = 3, Name = "Monster Energy (Original)", Category = "Energy Drink", CaffeinePer100Ml = 32, DefaultPortionMl = 500 },
    new Beverage { Id = 4, Name = "Monster Ultra (Fehér/Zero)", Category = "Energy Drink", CaffeinePer100Ml = 32, DefaultPortionMl = 500 }, // JAVÍTVA: 32-re
    new Beverage { Id = 5, Name = "Monster Mango Loco", Category = "Energy Drink", CaffeinePer100Ml = 32, DefaultPortionMl = 500 },
    new Beverage { Id = 6, Name = "Hell Energy Classic", Category = "Energy Drink", CaffeinePer100Ml = 32, DefaultPortionMl = 250 },
    new Beverage { Id = 7, Name = "Hell Energy Zero", Category = "Energy Drink", CaffeinePer100Ml = 32, DefaultPortionMl = 250 },
    new Beverage { Id = 8, Name = "Burn Original", Category = "Energy Drink", CaffeinePer100Ml = 32, DefaultPortionMl = 250 },
    new Beverage { Id = 9, Name = "Bomba! Classic", Category = "Energy Drink", CaffeinePer100Ml = 32, DefaultPortionMl = 250 },

    // --- ENERGIAITALOK (ERŐS / EXTRÉM) ---
    new Beverage { Id = 10, Name = "Hell Strong (Apple / Focus)", Category = "Energy Drink", CaffeinePer100Ml = 38.4, DefaultPortionMl = 250 },
    new Beverage { Id = 11, Name = "Hell Strong Watermelon", Category = "Energy Drink", CaffeinePer100Ml = 38.4, DefaultPortionMl = 250 },
    new Beverage { Id = 12, Name = "Reign Total Body Fuel", Category = "Energy Drink", CaffeinePer100Ml = 40, DefaultPortionMl = 500 },

    // --- DOBOZOS JEGESKÁVÉK ---
    new Beverage { Id = 13, Name = "Hell Ice Coffee Latte / Cappuccino", Category = "Ice Coffee", CaffeinePer100Ml = 40, DefaultPortionMl = 250 },
    new Beverage { Id = 14, Name = "Hell Ice Coffee Double Espresso", Category = "Ice Coffee", CaffeinePer100Ml = 48, DefaultPortionMl = 250 },
    new Beverage { Id = 15, Name = "Starbucks Frappuccino (üveges)", Category = "Ice Coffee", CaffeinePer100Ml = 30, DefaultPortionMl = 250 },
    new Beverage { Id = 16, Name = "Mizo Kávé (dobozos)", Category = "Ice Coffee", CaffeinePer100Ml = 25, DefaultPortionMl = 330 },

    // --- KÁVÉZÓI & OTTHONI KÁVÉK ---
    new Beverage { Id = 17, Name = "Espresso (Kávézós)", Category = "Coffee", CaffeinePer100Ml = 212, DefaultPortionMl = 30 },
    new Beverage { Id = 18, Name = "Dupla Espresso", Category = "Coffee", CaffeinePer100Ml = 212, DefaultPortionMl = 60 },
    new Beverage { Id = 19, Name = "Hosszú Kávé (Lungo)", Category = "Coffee", CaffeinePer100Ml = 60, DefaultPortionMl = 120 },
    new Beverage { Id = 20, Name = "Cappuccino", Category = "Coffee", CaffeinePer100Ml = 30, DefaultPortionMl = 200 }, 
    new Beverage { Id = 21, Name = "Filteres kávé (Bögre)", Category = "Coffee", CaffeinePer100Ml = 40, DefaultPortionMl = 250 },
    new Beverage { Id = 22, Name = "Instant Kávé (Nescafé, 1 bögre)", Category = "Coffee", CaffeinePer100Ml = 30, DefaultPortionMl = 200 },

    // --- KAPSZULÁS KÁVÉK ---
    new Beverage { Id = 23, Name = "Nespresso (Original Espresso kapszula)", Category = "Capsule", CaffeinePer100Ml = 162.5, DefaultPortionMl = 40 }, 
    new Beverage { Id = 24, Name = "Nespresso (Original Lungo kapszula)", Category = "Capsule", CaffeinePer100Ml = 75, DefaultPortionMl = 110 },   
    new Beverage { Id = 25, Name = "Dolce Gusto (Espresso kapszula)", Category = "Capsule", CaffeinePer100Ml = 200, DefaultPortionMl = 40 },    // JAVÍTVA: 200-ra (~80mg)
    new Beverage { Id = 26, Name = "Dolce Gusto (Lungo / Grande kapszula)", Category = "Capsule", CaffeinePer100Ml = 83, DefaultPortionMl = 120 }, 

    // --- ÜDÍTŐK / KÓLÁK ---
    new Beverage { Id = 27, Name = "Coca-Cola (Classic / Zero)", Category = "Soda", CaffeinePer100Ml = 9.6, DefaultPortionMl = 330 }, 
    new Beverage { Id = 28, Name = "Pepsi", Category = "Soda", CaffeinePer100Ml = 10.9, DefaultPortionMl = 330 },
    new Beverage { Id = 29, Name = "Pepsi Max", Category = "Soda", CaffeinePer100Ml = 12.8, DefaultPortionMl = 330 }, 
    new Beverage { Id = 30, Name = "Dr Pepper", Category = "Soda", CaffeinePer100Ml = 11.4, DefaultPortionMl = 330 },

    // --- TEÁK ---
    new Beverage { Id = 31, Name = "Fekete Tea (Bögre)", Category = "Tea", CaffeinePer100Ml = 20, DefaultPortionMl = 250 }, 
    new Beverage { Id = 32, Name = "Zöld Tea (Bögre)", Category = "Tea", CaffeinePer100Ml = 12, DefaultPortionMl = 250 }, 
    new Beverage { Id = 33, Name = "Yerba Mate", Category = "Tea", CaffeinePer100Ml = 35, DefaultPortionMl = 250 },
    new Beverage { Id = 34, Name = "Matcha (Tradicionális elkészítés)", Category = "Tea", CaffeinePer100Ml = 70, DefaultPortionMl = 100 }, 
    
    // --- ÚJ ---
    new Beverage { Id = 35, Name = "Mountain Dew", Category = "Soda", CaffeinePer100Ml = 15.2, DefaultPortionMl = 330 }, 
    new Beverage { Id = 36, Name = "Dolce Gusto (Iced Frappé)", Category = "Capsule", CaffeinePer100Ml = 53.3, DefaultPortionMl = 150 }, // JAVÍTVA: 53.3 (~80mg / adag)
    new Beverage { Id = 37, Name = "Dolce Gusto (Flat White)", Category = "Capsule", CaffeinePer100Ml = 55.5, DefaultPortionMl = 180 }, // JAVÍTVA: 55.5 (~100mg / adag)
    new Beverage { Id = 38, Name = "Dolce Gusto (Starbucks Caramel Macchiato)", Category = "Capsule", CaffeinePer100Ml = 40, DefaultPortionMl = 200 } // JAVÍTVA: 40 (~80mg / adag)
);
            
            #endregion
        }
    }
}