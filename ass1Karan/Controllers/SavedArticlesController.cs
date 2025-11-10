using ass1Karan.Models;
using ass1Karan.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ass1Karan.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class SavedArticlesController : Controller
    {
        private readonly StudentDBContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<SavedArticlesController> _logger;
        private readonly ActivityLogService _activityLogService;

        public SavedArticlesController(StudentDBContext context, UserManager<IdentityUser> userManager, ILogger<SavedArticlesController> logger, ActivityLogService activityLogService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _activityLogService = activityLogService;
        }

        [HttpPost]
        public async Task<IActionResult> Save(string title, string description, string url, string imageUrl)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Please log in before saving articles.";
                    return RedirectToAction("Login", "Account");
                }

                var exists = _context.SavedArticles.Any(a => a.Url == url && a.UserEmail == user.Email);
                if (exists)
                {
                    TempData["Message"] = "This article is already saved!";
                    return RedirectToAction("Index", "News");
                }

                var article = new SavedArticle
                {
                    Title = title,
                    Description = description,
                    Url = url,
                    ImageUrl = imageUrl,
                    UserEmail = user.Email,
                    SavedAt = DateTime.Now
                };

                _context.SavedArticles.Add(article);
                await _context.SaveChangesAsync();
                await _activityLogService.LogAsync(user.Id, $"Saved article '{title}'");

                TempData["Message"] = "📰 Article saved successfully!";
                return RedirectToAction("Index", "News");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving article.");
                TempData["ErrorMessage"] = "An error occurred while saving the article. Please try again.";
                return RedirectToAction("Index", "News");
            }
        }

        public async Task<IActionResult> MyArticles(int page = 1, string sort = "newest")
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Please log in to view your saved articles.";
                    return RedirectToAction("Login", "Account");
                }

                int pageSize = 8;
                var query = _context.SavedArticles
                    .Include(a => a.Comments)
                    .Include(a => a.Ratings)
                    .Where(a => a.UserEmail == user.Email);

                query = sort == "oldest" ? query.OrderBy(a => a.SavedAt) : query.OrderByDescending(a => a.SavedAt);

                var total = await query.CountAsync();
                var articles = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                ViewData["Page"] = page;
                ViewData["PageSize"] = pageSize;
                ViewData["Total"] = total;
                ViewData["Sort"] = sort;

                await _activityLogService.LogAsync(user.Id, $"Viewed saved articles page {page}");

                return View(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching saved articles.");
                TempData["ErrorMessage"] = "Unable to load your saved articles at the moment.";
                return RedirectToAction("Index", "News");
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminArticles()
        {
            try
            {
                var allArticles = await _context.SavedArticles
                    .Include(a => a.Comments)
                    .Include(a => a.Ratings)
                    .OrderByDescending(a => a.SavedAt)
                    .ToListAsync();

                ViewBag.TotalArticles = allArticles.Count;
                return View(allArticles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all saved articles for Admin.");
                TempData["ErrorMessage"] = "Unable to load articles.";
                return RedirectToAction("Dashboard", "Admin");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to delete an article.";
                    return RedirectToAction("Login", "Account");
                }

                var article = await _context.SavedArticles
                    .Include(a => a.Comments)
                    .Include(a => a.Ratings)
                    .FirstOrDefaultAsync(a => a.Id == id && (a.UserEmail == user.Email || User.IsInRole("Admin")));

                if (article != null)
                {
                    _context.Comments.RemoveRange(article.Comments);
                    _context.Ratings.RemoveRange(article.Ratings);
                    _context.SavedArticles.Remove(article);
                    await _context.SaveChangesAsync();
                    await _activityLogService.LogAsync(user.Id, $"Deleted article '{article.Title}'");
                    TempData["Message"] = "🗑️ Article deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Article not found or you don’t have permission to delete it.";
                }

                return RedirectToAction(User.IsInRole("Admin") ? "AdminArticles" : "MyArticles");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting article with ID {id}.");
                TempData["ErrorMessage"] = "Something went wrong while deleting the article.";
                return RedirectToAction(User.IsInRole("Admin") ? "AdminArticles" : "MyArticles");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostComment(int articleId, string content)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                if (string.IsNullOrWhiteSpace(content) || content.Length < 5 || content.Length > 250)
                {
                    TempData["CommentError"] = "Comment must be between 5 and 250 characters.";
                    return RedirectToAction(User.IsInRole("Admin") ? "AdminArticles" : "MyArticles");
                }

                var comment = new Comment
                {
                    Content = content,
                    AuthorEmail = user.Email,
                    SavedArticleId = articleId,
                    CreatedAt = DateTime.Now
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                await _activityLogService.LogAsync(user.Id, $"Added comment on article ID {articleId}");
                TempData["Message"] = "💬 Comment added!";
                return RedirectToAction(User.IsInRole("Admin") ? "AdminArticles" : "MyArticles");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment.");
                TempData["ErrorMessage"] = "Failed to post comment.";
                return RedirectToAction(User.IsInRole("Admin") ? "AdminArticles" : "MyArticles");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var comment = await _context.Comments.FindAsync(id);
                if (comment == null) return NotFound();

                if (comment.AuthorEmail != user.Email && !User.IsInRole("Admin")) return Forbid();

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                await _activityLogService.LogAsync(user.Id, $"Deleted comment ID {id}");
                TempData["Message"] = "🗑️ Comment deleted!";

                return RedirectToAction(User.IsInRole("Admin") ? "AdminArticles" : "MyArticles");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment.");
                TempData["ErrorMessage"] = "Unable to delete comment.";
                return RedirectToAction(User.IsInRole("Admin") ? "AdminArticles" : "MyArticles");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Rate(int articleId, int value)
        {
            try
            {
                if (value < 1 || value > 5) return BadRequest();

                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var existing = await _context.Ratings.FirstOrDefaultAsync(r => r.SavedArticleId == articleId && r.UserEmail == user.Email);

                if (existing != null)
                    existing.Value = value;
                else
                    _context.Ratings.Add(new Rating { SavedArticleId = articleId, UserEmail = user.Email, Value = value });

                await _context.SaveChangesAsync();
                await _activityLogService.LogAsync(user.Id, $"Rated article ID {articleId} with {value} stars");
                TempData["Message"] = "⭐ Rating submitted!";

                return RedirectToAction(User.IsInRole("Admin") ? "AdminArticles" : "MyArticles");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while rating article.");
                TempData["ErrorMessage"] = "Could not submit rating.";
                return RedirectToAction(User.IsInRole("Admin") ? "AdminArticles" : "MyArticles");
            }
        }
    }
}
