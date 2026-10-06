using Caffeine.Services;
namespace Caffeine.ViewModels;
public sealed record StatsViewModel(LifetimeStats Stats, List<int> Years, int? Year, DateTime Month, DateTime Today,
    List<HeatmapDay> MonthDays, List<HeatmapDay> YearDays);
