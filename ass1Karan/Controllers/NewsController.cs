using Microsoft.AspNetCore.Mvc;
using ass1Karan.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace ass1Karan.Controllers
{
    public class NewsController : Controller
    {
        private readonly NewsService _newsService;

        public NewsController(NewsService newsService)
        {
            _newsService = newsService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var email = HttpContext.Session.GetString("UserEmail");

                var articles = await _newsService.GetTopHeadlinesAsync();

                return View(articles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Index(): {ex.Message}");

                TempData["ErrorMessage"] = "Oops! Something went wrong while loading news. Please try again later.";
                return View("Error"); 
            }
        }

        [HttpPost]
        public IActionResult SaveArticle(string title, string url)
        {
            try
            {
                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(url))
                {
                    TempData["ErrorMessage"] = "Invalid article details.";
                    return RedirectToAction("Index");
                }

                TempData["Message"] = $"Article '{title}' saved successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveArticle(): {ex.Message}");
                TempData["ErrorMessage"] = "Something went wrong while saving the article.";
                return RedirectToAction("Index");
            }
        }
    }
}
