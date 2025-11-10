using ass1Karan.Models;
using ass1Karan.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ass1Karan.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ActivityLogService _activityLogService;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager, ActivityLogService activityLogService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _activityLogService = activityLogService;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string confirmPassword)
        {
            try
            {
                if (password != confirmPassword)
                {
                    ViewBag.Error = "Passwords do not match.";
                    return View();
                }

                if (password.Length < 8 || !password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
                {
                    ViewBag.Error = "Password must have at least 8 characters, 1 uppercase, 1 lowercase, and 1 digit.";
                    return View();
                }

                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    ViewBag.Error = "Email already registered.";
                    return View();
                }

                var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    if (await _roleManager.RoleExistsAsync("User"))
                        await _userManager.AddToRoleAsync(user, "User");

                    await _activityLogService.LogAsync(user.Id, $"Registered a new account ({user.Email}).");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    await _activityLogService.LogAsync(user.Id, $"User '{user.Email}' logged in after registration.");

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Error = string.Join("; ", result.Errors.Select(e => e.Description));
                    return View();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred during registration.";
                Console.WriteLine($"[Register Error]: {ex.Message}");
                return View();
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ViewBag.Error = "Email and password are required.";
                    return View();
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    ViewBag.Error = "Email not registered.";
                    return View();
                }

                var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    await _activityLogService.LogAsync(user.Id, $"User '{user.Email}' logged in.");

                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                        return RedirectToAction("Dashboard", "Admin");

                    return RedirectToAction("Index", "News");
                }

                ViewBag.Error = "Invalid credentials.";
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred during login.";
                Console.WriteLine($"[Login Error]: {ex.Message}");
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                    await _activityLogService.LogAsync(user.Id, $"User '{user.Email}' logged out.");

                await _signInManager.SignOutAsync();
                TempData["Message"] = "You have been logged out successfully.";
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while logging out.";
                Console.WriteLine($"[Logout Error]: {ex.Message}");
                return RedirectToAction("Login", "Account");
            }
        }
    }
}
