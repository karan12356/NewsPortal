using Microsoft.AspNetCore.Mvc;
using ass1Karan.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ass1Karan.Controllers
{
    public class SavedArticlesController : Controller
    {
        private readonly StudentDBContext _context;
        private readonly ILogger<SavedArticlesController> _logger;

        public SavedArticlesController(StudentDBContext context, ILogger<SavedArticlesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Save(string title, string description, string url, string imageUrl)
        {
            try
            {
                var email = HttpContext.Session.GetString("UserEmail");
                if (email == null)
                {
                    TempData["ErrorMessage"] = "Please log in before saving articles.";
                    return RedirectToAction("Login", "Account");
                }

                var article = new SavedArticle
                {
                    Title = title,
                    Description = description,
                    Url = url,
                    ImageUrl = imageUrl,
                    UserEmail = email
                };

                _context.SavedArticles.Add(article);
                await _context.SaveChangesAsync();

                TempData["Message"] = "📰 Article saved successfully!";
                return RedirectToAction("Index", "News");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving article.");
                TempData["ErrorMessage"] = "An error occurred while saving the article. Please try again later.";
                return RedirectToAction("Index", "News");
            }
        }

        public IActionResult MyArticles()
        {
            try
            {
                var email = HttpContext.Session.GetString("UserEmail");
                if (email == null)
                {
                    TempData["ErrorMessage"] = "Please log in to view your saved articles.";
                    return RedirectToAction("Login", "Account");
                }

                var myArticles = _context.SavedArticles
                    .Where(a => a.UserEmail == email)
                    .OrderByDescending(a => a.SavedAt)
                    .ToList();

                return View(myArticles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching saved articles.");
                TempData["ErrorMessage"] = "Unable to load your saved articles at the moment.";
                return RedirectToAction("Index", "News");
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                {
                    TempData["ErrorMessage"] = "You must be logged in to delete an article.";
                    return RedirectToAction("Login", "Account");
                }

                var article = _context.SavedArticles
                    .FirstOrDefault(a => a.Id == id && a.UserEmail == userEmail);

                if (article != null)
                {
                    _context.SavedArticles.Remove(article);
                    _context.SaveChanges();
                    TempData["Message"] = "🗑️ Article deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Article not found or you don’t have permission to delete it.";
                }

                return RedirectToAction("MyArticles");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting article with ID {id}.");
                TempData["ErrorMessage"] = "Something went wrong while deleting the article. Please try again.";
                return RedirectToAction("MyArticles");
            }
        }
    }
}
