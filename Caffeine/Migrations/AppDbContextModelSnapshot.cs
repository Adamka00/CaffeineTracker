
using System;
using Caffeine.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Caffeine.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "10.0.11");

            modelBuilder.Entity("Caffeine.Models.AppUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Caffeine.Models.Beverage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("CaffeinePer100Ml")
                        .HasColumnType("REAL");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("DefaultPortionMl")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Beverages");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            CaffeinePer100Ml = 32.0,
                            Category = "Energy Drink",
                            DefaultPortionMl = 250,
                            Name = "Red Bull (Classic / Sugarfree)"
                        },
                        new
                        {
                            Id = 2,
                            CaffeinePer100Ml = 32.0,
                            Category = "Energy Drink",
                            DefaultPortionMl = 250,
                            Name = "Red Bull Kókusz-Áfonya (Edition)"
                        },
                        new
                        {
                            Id = 3,
                            CaffeinePer100Ml = 32.0,
                            Category = "Energy Drink",
                            DefaultPortionMl = 500,
                            Name = "Monster Energy (Original)"
                        },
                        new
                        {
                            Id = 4,
                            CaffeinePer100Ml = 32.0,
                            Category = "Energy Drink",
                            DefaultPortionMl = 500,
                            Name = "Monster Ultra (Fehér/Zero)"
                        },
                        new
                        {
                            Id = 5,
                            CaffeinePer100Ml = 32.0,
                            Category = "Energy Drink",
                            DefaultPortionMl = 500,
                            Name = "Monster Mango Loco"
                        },
                        new
                        {
                            Id = 6,
                            CaffeinePer100Ml = 32.0,
                            Category = "Energy Drink",
                            DefaultPortionMl = 250,
                            Name = "Hell Energy Classic"
                        },
                        new
                        {
                            Id = 7,
                            CaffeinePer100Ml = 32.0,
                            Category = "Energy Drink",
                            DefaultPortionMl = 250,
                            Name = "Hell Energy Zero"
                        },
                        new
                        {
                            Id = 8,
                            CaffeinePer100Ml = 32.0,
                            Category = "Energy Drink",
                            DefaultPortionMl = 250,
                            Name = "Burn Original"
                        },
                        new
                        {
                            Id = 9,
                            CaffeinePer100Ml = 32.0,
                            Category = "Energy Drink",
                            DefaultPortionMl = 250,
                            Name = "Bomba! Classic"
                        },
                        new
                        {
                            Id = 10,
                            CaffeinePer100Ml = 38.399999999999999,
                            Category = "Energy Drink",
                            DefaultPortionMl = 250,
                            Name = "Hell Strong (Apple / Focus)"
                        },
                        new
                        {
                            Id = 11,
                            CaffeinePer100Ml = 38.399999999999999,
                            Category = "Energy Drink",
                            DefaultPortionMl = 250,
                            Name = "Hell Strong Watermelon"
                        },
                        new
                        {
                            Id = 12,
                            CaffeinePer100Ml = 40.0,
                            Category = "Energy Drink",
                            DefaultPortionMl = 500,
                            Name = "Reign Total Body Fuel"
                        },
                        new
                        {
                            Id = 13,
                            CaffeinePer100Ml = 40.0,
                            Category = "Ice Coffee",
                            DefaultPortionMl = 250,
                            Name = "Hell Ice Coffee Latte / Cappuccino"
                        },
                        new
                        {
                            Id = 14,
                            CaffeinePer100Ml = 48.0,
                            Category = "Ice Coffee",
                            DefaultPortionMl = 250,
                            Name = "Hell Ice Coffee Double Espresso"
                        },
                        new
                        {
                            Id = 15,
                            CaffeinePer100Ml = 30.0,
                            Category = "Ice Coffee",
                            DefaultPortionMl = 250,
                            Name = "Starbucks Frappuccino (üveges)"
                        },
                        new
                        {
                            Id = 16,
                            CaffeinePer100Ml = 25.0,
                            Category = "Ice Coffee",
                            DefaultPortionMl = 330,
                            Name = "Mizo Kávé (dobozos)"
                        },
                        new
                        {
                            Id = 17,
                            CaffeinePer100Ml = 212.0,
                            Category = "Coffee",
                            DefaultPortionMl = 30,
                            Name = "Espresso (Kávézós)"
                        },
                        new
                        {
                            Id = 18,
                            CaffeinePer100Ml = 212.0,
                            Category = "Coffee",
                            DefaultPortionMl = 60,
                            Name = "Dupla Espresso"
                        },
                        new
                        {
                            Id = 19,
                            CaffeinePer100Ml = 60.0,
                            Category = "Coffee",
                            DefaultPortionMl = 120,
                            Name = "Hosszú Kávé (Lungo)"
                        },
                        new
                        {
                            Id = 20,
                            CaffeinePer100Ml = 30.0,
                            Category = "Coffee",
                            DefaultPortionMl = 200,
                            Name = "Cappuccino"
                        },
                        new
                        {
                            Id = 21,
                            CaffeinePer100Ml = 40.0,
                            Category = "Coffee",
                            DefaultPortionMl = 250,
                            Name = "Filteres kávé (Bögre)"
                        },
                        new
                        {
                            Id = 22,
                            CaffeinePer100Ml = 30.0,
                            Category = "Coffee",
                            DefaultPortionMl = 200,
                            Name = "Instant Kávé (Nescafé, 1 bögre)"
                        },
                        new
                        {
                            Id = 23,
                            CaffeinePer100Ml = 162.5,
                            Category = "Capsule",
                            DefaultPortionMl = 40,
                            Name = "Nespresso (Original Espresso kapszula)"
                        },
                        new
                        {
                            Id = 24,
                            CaffeinePer100Ml = 75.0,
                            Category = "Capsule",
                            DefaultPortionMl = 110,
                            Name = "Nespresso (Original Lungo kapszula)"
                        },
                        new
                        {
                            Id = 25,
                            CaffeinePer100Ml = 200.0,
                            Category = "Capsule",
                            DefaultPortionMl = 40,
                            Name = "Dolce Gusto (Espresso kapszula)"
                        },
                        new
                        {
                            Id = 26,
                            CaffeinePer100Ml = 83.0,
                            Category = "Capsule",
                            DefaultPortionMl = 120,
                            Name = "Dolce Gusto (Lungo / Grande kapszula)"
                        },
                        new
                        {
                            Id = 27,
                            CaffeinePer100Ml = 9.5999999999999996,
                            Category = "Soda",
                            DefaultPortionMl = 330,
                            Name = "Coca-Cola (Classic / Zero)"
                        },
                        new
                        {
                            Id = 28,
                            CaffeinePer100Ml = 10.9,
                            Category = "Soda",
                            DefaultPortionMl = 330,
                            Name = "Pepsi"
                        },
                        new
                        {
                            Id = 29,
                            CaffeinePer100Ml = 12.800000000000001,
                            Category = "Soda",
                            DefaultPortionMl = 330,
                            Name = "Pepsi Max"
                        },
                        new
                        {
                            Id = 30,
                            CaffeinePer100Ml = 11.4,
                            Category = "Soda",
                            DefaultPortionMl = 330,
                            Name = "Dr Pepper"
                        },
                        new
                        {
                            Id = 31,
                            CaffeinePer100Ml = 20.0,
                            Category = "Tea",
                            DefaultPortionMl = 250,
                            Name = "Fekete Tea (Bögre)"
                        },
                        new
                        {
                            Id = 32,
                            CaffeinePer100Ml = 12.0,
                            Category = "Tea",
                            DefaultPortionMl = 250,
                            Name = "Zöld Tea (Bögre)"
                        },
                        new
                        {
                            Id = 33,
                            CaffeinePer100Ml = 35.0,
                            Category = "Tea",
                            DefaultPortionMl = 250,
                            Name = "Yerba Mate"
                        },
                        new
                        {
                            Id = 34,
                            CaffeinePer100Ml = 70.0,
                            Category = "Tea",
                            DefaultPortionMl = 100,
                            Name = "Matcha (Tradicionális elkészítés)"
                        },
                        new
                        {
                            Id = 35,
                            CaffeinePer100Ml = 15.199999999999999,
                            Category = "Soda",
                            DefaultPortionMl = 330,
                            Name = "Mountain Dew"
                        },
                        new
                        {
                            Id = 36,
                            CaffeinePer100Ml = 53.299999999999997,
                            Category = "Capsule",
                            DefaultPortionMl = 150,
                            Name = "Dolce Gusto (Iced Frappé)"
                        },
                        new
                        {
                            Id = 37,
                            CaffeinePer100Ml = 55.5,
                            Category = "Capsule",
                            DefaultPortionMl = 180,
                            Name = "Dolce Gusto (Flat White)"
                        },
                        new
                        {
                            Id = 38,
                            CaffeinePer100Ml = 40.0,
                            Category = "Capsule",
                            DefaultPortionMl = 200,
                            Name = "Dolce Gusto (Starbucks Caramel Macchiato)"
                        });
                });

            modelBuilder.Entity("Caffeine.Models.CaffeineLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BeverageId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ConsumedAmountMl")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ConsumedAt")
                        .HasColumnType("TEXT");

                    b.Property<double>("TotalCaffeineMg")
                        .HasColumnType("REAL");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("BeverageId");

                    b.ToTable("CaffeineLogs");
                });

            modelBuilder.Entity("Caffeine.Models.CaffeineLog", b =>
                {
                    b.HasOne("Caffeine.Models.Beverage", "Beverage")
                        .WithMany()
                        .HasForeignKey("BeverageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Beverage");
                });
#pragma warning restore 612, 618
        }
    }
}
