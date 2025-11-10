using ass1Karan.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ass1Karan.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ActivityLogController : Controller
    {
        private readonly StudentDBContext _context;

        public ActivityLogController(StudentDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var logs = await _context.ActivityLogs
                    .OrderByDescending(a => a.Timestamp)
                    .Take(50)
                    .ToListAsync();

                return View(logs);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to load activity logs. Please try again later.";
                Console.WriteLine($"[ActivityLog Error]: {ex.Message}");
                return View(new List<ActivityLog>());
            }
        }
    }
}
