using Caffeine.Models;
using Caffeine.Repositories;
using Caffeine.Services;
using Caffeine.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Caffeine.Data;
using Caffeine.ViewModels;
using Microsoft.AspNetCore.Localization; // Ha a Beverages-hez egyelőre a kontextust használjuk

namespace Caffeine.Controllers
{
    public class TrackerController : Controller
    {
        private readonly ICaffeineLogRepository _logRepository;
        private readonly ICaffeineCalculatorService _calculatorService;
        private readonly AppDbContext _context; // Itt használhatnánk IBeverageRepository-t is a tisztaság kedvéért

        public TrackerController(
            ICaffeineLogRepository logRepository, 
            ICaffeineCalculatorService calculatorService,
            AppDbContext context)
        {
            _logRepository = logRepository;
            _calculatorService = calculatorService;
            _context = context;
        }

        // --- DASHBOARD (Főoldal) ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
    
            // UI-hoz: Csak a mai naptári nap (éjféltől mostanáig) a listázáshoz
            var todayLogs = await _logRepository.GetLogsForDateAsync(now);
    
            // Számoláshoz: Az elmúlt 24 óra! (Itt van Junnie javítása)
            var activeLogs = await _logRepository.GetLogsSinceAsync(now.AddHours(-24));

            var viewModel = new DashboardViewModel
            {
                TodayLogs = todayLogs,
                // Napi limitbe (pl 400mg) csak a MA megivott mennyiség számít bele
                TotalConsumedTodayMg = todayLogs.Sum(l => l.TotalCaffeineMg),
        
                // DE a vérben lévő szinthez az aktív (elmúlt 24 órás) naplót használjuk!
                CurrentActiveCaffeineMg = Math.Round(_calculatorService.GetCurrentTotalActiveCaffeine(activeLogs, now), 1),
                SleepReadinessTime = _calculatorService.EstimateSleepReadiness(activeLogs, now, 25.0)
            };

            // 24 órás Chart adatpontok (00:00 - 23:59)
            var startOfDay = now.Date;
            for (int i = 0; i < 48; i++)
            {
                var timePoint = startOfDay.AddMinutes(i * 30);
        
                // A görbe kirajzolásánál is a 24 órás adatokat használjuk a pontos átfedésekért
                var activeMg = _calculatorService.GetCurrentTotalActiveCaffeine(activeLogs, timePoint);
        
                viewModel.ChartData.Add(new ChartDataPoint
                {
                    TimeLabel = timePoint.ToString("HH:mm"),
                    ActiveCaffeine = Math.Round(activeMg, 1)
                });
            }

            return View(viewModel);
        }

        // --- ÚJ ITAL RÖGZÍTÉSE (Űrlap betöltése) ---
        [HttpGet]
        public async Task<IActionResult> LogDrink()
        {
            var beverages = await _context.Beverages.OrderBy(b => b.Name).ToListAsync();
            
            var viewModel = new LogDrinkFormViewModel
            {
                // A ViewBag/ViewData helyett szigorúan típusosan adjuk át a listát!
                BeverageOptions = beverages.Select(b => new SelectListItem 
                { 
                    Value = b.Id.ToString(), 
                    Text = b.Name 
                })
            };

            return View(viewModel);
        }

        // --- ÚJ ITAL RÖGZÍTÉSE (Adatküldés) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogDrink(LogDrinkFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Hiba esetén újra kell tölteni a dropdown listát
                var beverages = await _context.Beverages.OrderBy(b => b.Name).ToListAsync();
                model.BeverageOptions = beverages.Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name });
                return View(model);
            }

            // Kikeressük a kiválasztott italt, hogy megtudjuk a koffeintartalmát
            var selectedBeverage = await _context.Beverages.FindAsync(model.SelectedBeverageId);
            if (selectedBeverage == null) return NotFound();

            // Kiszámoljuk a pontos koffeinmennyiséget: (Mg/100ml * elfogyasztott mennyiség / 100)
            double calculatedCaffeine = (selectedBeverage.CaffeinePer100Ml * model.AmountMl) / 100.0;

            // Létrehozzuk a Domain Entitást
            var newLog = new CaffeineLog
            {
                BeverageId = selectedBeverage.Id,
                ConsumedAmountMl = model.AmountMl,
                ConsumedAt = model.ConsumedAt,
                TotalCaffeineMg = calculatedCaffeine
            };

            // Mentés az adatbázisba a Repository-n keresztül
            await _logRepository.AddLogAsync(newLog);

            // Vissza a Dashboardra
            return RedirectToAction(nameof(Index));
        }

        // --- TÖRLÉS (Ha véletlenül rögzített valamit) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLog(int id)
        {
            await _logRepository.DeleteLogAsync(id);
            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) } // 1 évig megjegyzi
            );

            return LocalRedirect(returnUrl);
        }
    }
    
    
}