using System;
using System.Diagnostics;
using ass1Karan.Models;
using ass1Karan.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ass1Karan.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ActivityLogService _activityLogService;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ActivityLogService activityLogService, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _activityLogService = activityLogService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                    await _activityLogService.LogAsync(user.Id, $"Visited Home page.");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading the Home page (Index).");
                TempData["ErrorMessage"] = "Something went wrong while loading the home page. Please try again later.";
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> Privacy()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                    await _activityLogService.LogAsync(user.Id, $"Visited Privacy page.");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading the Privacy page.");
                TempData["ErrorMessage"] = "Something went wrong while loading the privacy page.";
                return RedirectToAction("Error");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            try
            {
                var errorViewModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                };
                return View(errorViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Critical error occurred in the Error page rendering.");
                return Content("An unexpected system error occurred. Please contact support.");
            }
        }

        public IActionResult Error404()
        {
            Response.StatusCode = 404;
            return View();
        }

        public IActionResult Error500()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            ViewBag.ErrorMessage = exceptionDetails?.Error.Message ?? "An unexpected server error occurred.";
            Response.StatusCode = 500;
            return View();
        }
    }
}
