using Caffeine.Models;
using Caffeine.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caffeine.ViewModels
{
    public class DashboardViewModel
    {
        public StreakSummary Streak { get; set; } =
            new(0, 0, false);

        public bool IsCaffeineFreeDay { get; set; }

        public bool HasCaffeineToday =>
            TodayLogs.Any(l =>
                l.TotalCaffeineMg > 0);

        public DateTime SelectedDate { get; set; }

        public DateTime Today { get; set; }

        public bool IsToday =>
            SelectedDate == Today;

        public List<FavoriteDrink> Favorites { get; set; } = [];


        public IEnumerable<CaffeineLog> TodayLogs { get; set; } =
            new List<CaffeineLog>();

        public double TotalConsumedTodayMg { get; set; }

        public double CurrentActiveCaffeineMg { get; set; }

        public DateTime? SleepReadinessTime { get; set; }


        public string TargetSleepTimeStr { get; set; } =
            "23:00";

        public double CaffeineAtTargetSleepTime { get; set; }

        public double TargetMg { get; set; } =
            25;

        public string SleepQualityKey { get; set; } =
            string.Empty;

        public string SleepQualityColor { get; set; } =
            string.Empty;


        public List<ChartDataPoint> ChartData { get; set; } =
            new List<ChartDataPoint>();


        public string StatusColor
        {
            get
            {
                if (CurrentActiveCaffeineMg < 25)
                    return "text-emerald-400";

                if (CurrentActiveCaffeineMg < 150)
                    return "text-cyan-400";

                if (CurrentActiveCaffeineMg < 300)
                    return "text-yellow-400";

                return "text-rose-500 text-shadow-[0_0_15px_rgba(244,63,94,0.5)]";
            }
        }
    }


    public class ChartDataPoint
    {
        public string TimeLabel { get; set; } =
            string.Empty;

        public double ActiveCaffeine { get; set; }
    }
}