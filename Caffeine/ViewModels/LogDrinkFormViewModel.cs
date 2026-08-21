using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Caffeine.ViewModels
{
    public class LogDrinkFormViewModel
    {
        [Required(ErrorMessage = "Kérlek válassz egy italt!")]
        [Display(Name = "Mit ittál?")]
        public int SelectedBeverageId { get; set; }

        [Required(ErrorMessage = "Az elfogyasztott mennyiség megadása kötelező!")]
        [Range(1, 2000, ErrorMessage = "A mennyiségnek 1 és 2000 ml között kell lennie.")]
        [Display(Name = "Mennyiség (ml)")]
        public int AmountMl { get; set; }

        [Required]
        [Display(Name = "Mikor?")]
        public DateTime ConsumedAt { get; set; } = DateTime.Now;

        // A legördülő menü (Select2 vagy natív dropdown) elemei
        public IEnumerable<SelectListItem> BeverageOptions { get; set; } = new List<SelectListItem>();
    }
}