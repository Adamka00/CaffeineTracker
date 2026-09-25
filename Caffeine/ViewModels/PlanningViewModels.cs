using System.ComponentModel.DataAnnotations;
using Caffeine.Models;
using Caffeine.Services;

namespace Caffeine.ViewModels;

public sealed class PlanningForm
{
    public int? BeverageId { get; set; }

    [Range(1, 2000)]
    public int AmountMl { get; set; } = 250;

    [Range(0, 2000)]
    public int DoseMg { get; set; } = 80;

    [Required]
    public DateTime? Bedtime { get; set; }

    [Required]
    public DateTime? IntakeAt { get; set; }

    [Range(1, 200)]
    public int TargetMg { get; set; } = 25;

    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
    public List<Beverage> Beverages { get; set; } = [];

    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
    public PlanningResult? Result { get; set; }
}

public sealed class PreferenceForm
{
    [Required]
    public string PlannedBedtime { get; set; } = "23:00";

    [Range(1, 200)]
    public int TargetMg { get; set; } = 25;
}