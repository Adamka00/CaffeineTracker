using Caffeine.Data;
using Caffeine.Models;
using Caffeine.Repositories;
using Caffeine.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Caffeine.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICaffeineLogRepository _logRepository;
        private readonly PasswordHasher<AppUser> _passwordHasher;

        public AccountController(AppDbContext context, ICaffeineLogRepository logRepository)
        {
            _context = context;
            _logRepository = logRepository;
            _passwordHasher = new PasswordHasher<AppUser>();
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Ezzel az email címmel már regisztráltak!");
                return View(model);
            }

            var user = new AppUser
            {
                Username = model.Username,
                Email = model.Email
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await SignInUserAsync(user);
            await TransferGuestLogs(user.Id.ToString());

            return RedirectToAction("Index", "Tracker");
        }

        [HttpGet]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Hibás email vagy jelszó!");
                return View(model);
            }


            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Hibás email vagy jelszó!");
                return View(model);
            }

            await SignInUserAsync(user);
            await TransferGuestLogs(user.Id.ToString());

            return RedirectToAction("Index", "Tracker");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Tracker");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {

                await _logRepository.DeleteAllLogsForUserAsync(userId);


                var user = await _context.Users.FindAsync(int.Parse(userId));
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }


                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return RedirectToAction("Index", "Tracker");
        }

        private async Task SignInUserAsync(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            });
        }


        private async Task TransferGuestLogs(string newUserId)
        {
            var guestId = Request.Cookies["GuestId"];
            if (!string.IsNullOrEmpty(guestId))
            {
                await _logRepository.TransferLogsAsync(guestId, newUserId);
                Response.Cookies.Delete("GuestId");
            }
        }
    }
}