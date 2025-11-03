using System;
using System.Diagnostics;
using ass1Karan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ass1Karan.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading the Home page (Index).");
                TempData["ErrorMessage"] = "Something went wrong while loading the home page. Please try again later.";
                return RedirectToAction("Error");
            }
        }

        public IActionResult Privacy()
        {
            try
            {
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
    }
}
