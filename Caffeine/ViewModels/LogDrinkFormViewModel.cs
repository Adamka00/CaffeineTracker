using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Caffeine.ViewModels
{
    public class LogDrinkFormViewModel
    {
        // 1. Kapcsoló: Egyedi italt akar-e felvinni?
        public bool IsCustomDrink { get; set; }

        // 2. Kérdőjeles lett (int?), így nem kötelező kitölteni, ha egyedit visz fel!
        [Display(Name = "Választott Ital")]
        public int? SelectedBeverageId { get; set; }

        // --- ÚJ MEZŐK EGYEDI ITALHOZ ---
        [Display(Name = "Saját ital neve")]
        public string? CustomBeverageName { get; set; }

        [Display(Name = "Koffein (mg) / 100ml")]
        public double? CustomCaffeinePer100Ml { get; set; }

        // --- KÖZÖS MEZŐK ---
        [Required(ErrorMessage = "Az elfogyasztott mennyiség megadása kötelező!")]
        [Range(1, 2000, ErrorMessage = "A mennyiségnek 1 és 2000 ml között kell lennie.")]
        [Display(Name = "Mennyiség (ml)")]
        public int AmountMl { get; set; }

        [Required]
        [Display(Name = "Mikor?")]
        public DateTime ConsumedAt { get; set; } = DateTime.Now;

        public IEnumerable<SelectListItem> BeverageOptions { get; set; } = new List<SelectListItem>();
    }
}