using Caffeine.Data;
using Caffeine.Models;
using Caffeine.Repositories;
using Caffeine.Services;
using Caffeine.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Caffeine.Controllers
{
    public class TrackerController : Controller
    {
        private readonly ICaffeineLogRepository _logRepository;
        private readonly ICaffeineCalculatorService _calculatorService;
        private readonly AppDbContext _context;

        public TrackerController(
            ICaffeineLogRepository logRepository,
            ICaffeineCalculatorService calculatorService,
            AppDbContext context)
        {
            _logRepository = logRepository;
            _calculatorService = calculatorService;
            _context = context;
        }


        private string GetCurrentUserId()
        {

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return User.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            }


            var guestId = Request.Cookies["GuestId"];
            if (string.IsNullOrEmpty(guestId))
            {

                guestId = "Guest_" + Guid.NewGuid().ToString();
                Response.Cookies.Append("GuestId", guestId, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });
            }
            return guestId;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var userId = GetCurrentUserId();


            var todayLogs = await _logRepository.GetLogsForDateAsync(now, userId);
            var activeLogs = await _logRepository.GetLogsSinceAsync(now.AddHours(-24), userId);


            string targetTimeStr = Request.Cookies["TargetSleepTime"] ?? "23:00";
            if (!TimeSpan.TryParse(targetTimeStr, out TimeSpan parsedTime))
            {
                parsedTime = new TimeSpan(23, 0, 0);
            }

            DateTime targetSleepDateTime = now.Date.Add(parsedTime);
            if (targetSleepDateTime < now)
            {
                targetSleepDateTime = targetSleepDateTime.AddDays(1);
            }

            double caffeineAtSleep = Math.Round(_calculatorService.GetCurrentTotalActiveCaffeine(activeLogs, targetSleepDateTime), 1);

            string qualityKey, qualityColor;
            if (caffeineAtSleep < 10) {
                qualityKey = "Tökéletes, mély alvás várható.";
                qualityColor = "text-emerald-400";
            } else if (caffeineAtSleep <= 25) {
                qualityKey = "Jó alvás, a küszöb alatt vagy.";
                qualityColor = "text-cyan-400";
            } else if (caffeineAtSleep <= 50) {
                qualityKey = "Felszínesebb alvás, forgolódás várható.";
                qualityColor = "text-yellow-400";
            } else {
                qualityKey = "Nehéz elalvás, megzavart pihenés!";
                qualityColor = "text-rose-500";
            }

            var viewModel = new DashboardViewModel
            {
                TodayLogs = todayLogs,
                TotalConsumedTodayMg = Math.Round(todayLogs.Sum(l => l.TotalCaffeineMg), 1),
                CurrentActiveCaffeineMg = Math.Round(_calculatorService.GetCurrentTotalActiveCaffeine(activeLogs, now), 1),
                SleepReadinessTime = _calculatorService.EstimateSleepReadiness(activeLogs, now, 25.0),
                TargetSleepTimeStr = targetTimeStr,
                CaffeineAtTargetSleepTime = caffeineAtSleep,
                SleepQualityKey = qualityKey,
                SleepQualityColor = qualityColor
            };


            var startOfDay = now.Date;
            for (int i = 0; i < 48; i++)
            {
                var timePoint = startOfDay.AddMinutes(i * 30);
                var activeMg = _calculatorService.GetCurrentTotalActiveCaffeine(activeLogs, timePoint);

                viewModel.ChartData.Add(new ChartDataPoint
                {
                    TimeLabel = timePoint.ToString("HH:mm"),
                    ActiveCaffeine = Math.Round(activeMg, 1)
                });
            }

            return View(viewModel);
        }


        [HttpPost]
        public IActionResult SetTargetSleepTime(string time)
        {
            if (TimeSpan.TryParse(time, out _))
            {
                Response.Cookies.Append("TargetSleepTime", time, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> LogDrink()
        {
            var beverages = await _context.Beverages.OrderBy(b => b.Name).ToListAsync();
            var viewModel = new LogDrinkFormViewModel
            {
                BeverageOptions = beverages.Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name })
            };
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogDrink(LogDrinkFormViewModel model)
        {
            if (model.IsCustomDrink)
            {
                if (string.IsNullOrWhiteSpace(model.CustomBeverageName) || model.CustomCaffeinePer100Ml == null || model.CustomCaffeinePer100Ml <= 0)
                {
                    ModelState.AddModelError("", "Kérlek add meg a saját ital nevét és koffeintartalmát (mg/100ml)!");
                }
            }
            else
            {
                if (model.SelectedBeverageId == null || model.SelectedBeverageId == 0)
                {
                    ModelState.AddModelError("SelectedBeverageId", "Kérlek válassz egy italt a listából, vagy pipáld be az Egyedi ital opciót!");
                }
            }

            if (!ModelState.IsValid)
            {
                var beverages = await _context.Beverages.OrderBy(b => b.Name).ToListAsync();
                model.BeverageOptions = beverages.Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name });
                return View(model);
            }

            Beverage beverageToLog;
            double calculatedCaffeine;

            if (model.IsCustomDrink)
            {
                beverageToLog = new Beverage
                {
                    Name = model.CustomBeverageName!,
                    Category = "Custom",
                    CaffeinePer100Ml = model.CustomCaffeinePer100Ml!.Value,
                    DefaultPortionMl = model.AmountMl
                };

                _context.Beverages.Add(beverageToLog);
                await _context.SaveChangesAsync();

                calculatedCaffeine = (model.CustomCaffeinePer100Ml.Value * model.AmountMl) / 100.0;
            }
            else
            {
                beverageToLog = await _context.Beverages.FindAsync(model.SelectedBeverageId);
                if (beverageToLog == null) return NotFound();

                calculatedCaffeine = (beverageToLog.CaffeinePer100Ml * model.AmountMl) / 100.0;
            }

            var newLog = new CaffeineLog
            {
                BeverageId = beverageToLog.Id,
                ConsumedAmountMl = model.AmountMl,
                ConsumedAt = model.ConsumedAt,
                TotalCaffeineMg = Math.Round(calculatedCaffeine, 1),
                UserId = GetCurrentUserId()
            };

            await _logRepository.AddLogAsync(newLog);
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLog(int id)
        {

            await _logRepository.DeleteLogAsync(id, GetCurrentUserId());
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
    }
}