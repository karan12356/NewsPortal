using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ass1Karan.Models;
using ass1Karan.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ass1Karan.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly StudentDBContext _context;
        private readonly ActivityLogService _activityLogService;

        public AdminController(UserManager<IdentityUser> userManager, StudentDBContext context, ActivityLogService activityLogService)
        {
            _userManager = userManager;
            _context = context;
            _activityLogService = activityLogService;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                if (_userManager == null || _context == null)
                {
                    ViewBag.TotalUsers = 0;
                    ViewBag.SavedArticles = 0;
                    ViewBag.Users = new List<object>();
                    ViewBag.RecentLogs = new List<ActivityLog>();
                    return View();
                }

                var users = _userManager.Users.ToList();
                var userList = new List<dynamic>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userList.Add(new
                    {
                        Email = user.Email,
                        Role = roles.FirstOrDefault() ?? "None"
                    });
                }

                var recentLogs = await _context.ActivityLogs
                    .OrderByDescending(x => x.Timestamp)
                    .Take(10)
                    .ToListAsync();

                ViewBag.TotalUsers = users.Count;
                ViewBag.SavedArticles = _context.SavedArticles.Count();
                ViewBag.Users = userList;
                ViewBag.TotalActivities = await _context.ActivityLogs.CountAsync();
                ViewBag.RecentLogs = recentLogs;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading Admin Dashboard. Please try again later.";
                Console.WriteLine($"[Admin Dashboard Error]: {ex.Message}");
                ViewBag.Users = new List<object>();
                ViewBag.RecentLogs = new List<ActivityLog>();
                ViewBag.TotalUsers = 0;
                ViewBag.SavedArticles = 0;
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> PromoteToAdmin(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    TempData["ErrorMessage"] = "Invalid email address.";
                    return RedirectToAction("Dashboard");
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    await _userManager.RemoveFromRoleAsync(user, "User");
                    await _userManager.AddToRoleAsync(user, "Admin");
                    await _activityLogService.LogAsync(user.Id, $"User '{user.Email}' promoted to Admin by another Admin.");
                    TempData["Message"] = $"{user.Email} promoted to Admin successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "User not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error promoting user to Admin.";
                Console.WriteLine($"[PromoteToAdmin Error]: {ex.Message}");
            }

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> DemoteToUser(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    TempData["ErrorMessage"] = "Invalid email address.";
                    return RedirectToAction("Dashboard");
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
                    await _userManager.AddToRoleAsync(user, "User");
                    await _activityLogService.LogAsync(user.Id, $"Admin '{user.Email}' demoted back to User by another Admin.");
                    TempData["Message"] = $"{user.Email} demoted to User successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "User not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error demoting user.";
                Console.WriteLine($"[DemoteToUser Error]: {ex.Message}");
            }

            return RedirectToAction("Dashboard");
        }
    }
}
