using ass1Karan.Models;
using ass1Karan.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ass1Karan.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly StudentDBContext _context;
        private readonly ActivityLogService _activityLogService;

        public ProfileController(UserManager<IdentityUser> userManager, StudentDBContext context, ActivityLogService activityLogService)
        {
            _userManager = userManager;
            _context = context;
            _activityLogService = activityLogService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == user.Id);
                if (profile == null)
                {
                    profile = new UserProfile
                    {
                        UserId = user.Id,
                        FullName = user.UserName?.Split('@')[0],
                        JoinedDate = DateTime.Now
                    };
                    _context.UserProfiles.Add(profile);
                    await _context.SaveChangesAsync();
                    await _activityLogService.LogAsync(user.Id, $"Created profile for user {user.Email}");
                }

                await _activityLogService.LogAsync(user.Id, $"Visited Profile page");
                ViewBag.Email = user.Email;
                return View(profile);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading profile. Please try again later.";
                Console.WriteLine($"[Profile Index Error]: {ex.Message}");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UserProfile model, IFormFile profileImage)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == user.Id);
                if (profile == null)
                {
                    TempData["ErrorMessage"] = "Profile not found.";
                    return RedirectToAction("Index");
                }

                profile.FullName = model.FullName;

                if (profileImage != null && profileImage.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(profileImage.FileName)}";
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                    var filePath = Path.Combine(uploads, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(stream);
                    }
                    profile.ProfileImageUrl = "/uploads/" + fileName;
                }

                _context.UserProfiles.Update(profile);
                await _context.SaveChangesAsync();
                await _activityLogService.LogAsync(user.Id, $"Updated profile");

                TempData["Message"] = "Profile updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating profile. Please try again later.";
                Console.WriteLine($"[Profile Update Error]: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            try
            {
                if (newPassword != confirmPassword)
                {
                    TempData["ErrorMessage"] = "Passwords do not match.";
                    return RedirectToAction("Index");
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                if (result.Succeeded)
                {
                    await _activityLogService.LogAsync(user.Id, $"Changed password");
                    TempData["Message"] = "Password changed successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error changing password. Please try again.";
                Console.WriteLine($"[Password Change Error]: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
    }
}
