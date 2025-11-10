using Microsoft.AspNetCore.Mvc;
using ass1Karan.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace ass1Karan.Controllers
{
    public class NewsController : Controller
    {
        private readonly NewsService _newsService;
        private readonly ActivityLogService _activityLogService;
        private readonly UserManager<IdentityUser> _userManager;

        public NewsController(NewsService newsService, ActivityLogService activityLogService, UserManager<IdentityUser> userManager)
        {
            _newsService = newsService;
            _activityLogService = activityLogService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string category = "", string q = "", string sort = "newest", int page = 1)
        {
            try
            {
                int pageSize = 8;
                var (articles, total) = await _newsService.GetTopHeadlinesAsync(
                    string.IsNullOrWhiteSpace(category) ? null : category,
                    string.IsNullOrWhiteSpace(q) ? null : q,
                    page, pageSize);

                if (sort == "oldest") articles = articles.OrderBy(a => a.PublishedAt).ToList();
                else articles = articles.OrderByDescending(a => a.PublishedAt).ToList();

                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    if (!string.IsNullOrWhiteSpace(category))
                        await _activityLogService.LogAsync(user.Id, $"Filtered news by category '{category}'.");
                    if (!string.IsNullOrWhiteSpace(q))
                        await _activityLogService.LogAsync(user.Id, $"Searched news with keyword '{q}'.");
                }

                ViewData["Category"] = category;
                ViewData["Query"] = q;
                ViewData["Sort"] = sort;
                ViewData["Page"] = page;
                ViewData["PageSize"] = pageSize;
                ViewData["TotalResults"] = total;
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
        public async Task<IActionResult> SaveArticle(string title, string url, string imageUrl, string description)
        {
            try
            {
                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(url))
                {
                    TempData["ErrorMessage"] = "Invalid article details.";
                    return RedirectToAction("Index");
                }

                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                    await _activityLogService.LogAsync(user.Id, $"Saved article '{title}'.");

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
