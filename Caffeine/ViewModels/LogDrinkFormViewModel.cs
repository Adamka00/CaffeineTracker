using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Caffeine.ViewModels
{
    public class LogDrinkFormViewModel
    {
        public bool SaveAsFavorite { get; set; }
        public List<Caffeine.Models.Beverage> Beverages { get; set; } = [];

        public bool IsCustomDrink { get; set; }

        [Display(Name = "Választott Ital")]
        public int? SelectedBeverageId { get; set; }

        [Display(Name = "Saját ital neve")]
        [StringLength(120)]
        public string? CustomBeverageName { get; set; }

        [Display(Name = "Koffein (mg) / 100ml")]
        [Microsoft.AspNetCore.Mvc.ModelBinder(
            BinderType = typeof(Caffeine.Services.CaffeineAmountBinder))]
        [Range(0, 1000)]
        public double? CustomCaffeinePer100Ml { get; set; }

        [Required(ErrorMessage = "Az elfogyasztott mennyiség megadása kötelező!")]
        [Range(1, 2000, ErrorMessage = "A mennyiségnek 1 és 2000 ml között kell lennie.")]
        [Display(Name = "Mennyiség (ml)")]
        public int AmountMl { get; set; }

        [Required]
        [Display(Name = "Mikor?")]
        public DateTime ConsumedAt { get; set; } = DateTime.Now;

        public IEnumerable<SelectListItem> BeverageOptions { get; set; }
            = new List<SelectListItem>();
    }
}