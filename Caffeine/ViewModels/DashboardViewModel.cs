using Caffeine.Models;
using System;
using System.Collections.Generic;

namespace Caffeine.ViewModels
{
    public class DashboardViewModel
    {
        // Napi összes fogyasztás (pl. 400 mg a limit)
        public double TotalConsumedTodayMg { get; set; }
        
        // Jelenleg pörgő aktív koffein a vérben
        public double CurrentActiveCaffeineMg { get; set; }
        
        // Mikorra esik 25mg alá?
        public DateTime? SleepReadinessTime { get; set; }
        
        // Mai fogyasztások listája a "History" kártyákhoz
        public IEnumerable<CaffeineLog> TodayLogs { get; set; } = new List<CaffeineLog>();

        // Chart.js számára generált adatpontok (Időpont -> Koffein szint)
        public List<ChartDataPoint> ChartData { get; set; } = new List<ChartDataPoint>();
        
        // Színkód a UI-nak (pl. zöld, sárga, piros) a jelenlegi szint alapján
        public string StatusColor => CurrentActiveCaffeineMg switch
        {
            < 50 => "text-emerald-400 drop-shadow-[0_0_10px_rgba(52,211,153,0.8)]", // Nyugodt
            < 200 => "text-cyan-400 drop-shadow-[0_0_10px_rgba(34,211,238,0.8)]",   // Fókusz
            _ => "text-rose-500 drop-shadow-[0_0_15px_rgba(244,63,94,0.9)]"         // Túlpörgés
        };
    }

    // Segédosztály a grafikonhoz
    public class ChartDataPoint
    {
        public string TimeLabel { get; set; } = string.Empty; // pl "14:00"
        public double ActiveCaffeine { get; set; }
    }
}