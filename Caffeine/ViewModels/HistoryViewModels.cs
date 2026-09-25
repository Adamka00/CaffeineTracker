using Caffeine.Models;

namespace Caffeine.ViewModels;

public class WeekViewModel
{
    public DateTime Start { get; set; }

    public DateTime Today { get; set; }

    public List<DaySummary> Days { get; set; } = [];


    public double Total =>
        Days.Sum(d => d.TotalMg);


    public int ElapsedDays =>
        Days.Count(d => d.Date <= Today);


    public double Average =>
        ElapsedDays == 0
            ? 0
            : Total / ElapsedDays;


    public int LoggedDays =>
        Days.Count(d =>
            d.Count > 0 ||
            d.IsCaffeineFree);


    public int DrinkCount =>
        Days.Sum(d => d.Count);


    public DaySummary? Highest =>
        Days
            .Where(d => d.Count > 0)
            .OrderByDescending(d => d.TotalMg)
            .FirstOrDefault();


    public Beverage? MostFrequent { get; set; }
}


public class DaySummary
{
    public bool IsCaffeineFree { get; set; }

    public DateTime Date { get; set; }

    public double TotalMg { get; set; }

    public int Count { get; set; }
}