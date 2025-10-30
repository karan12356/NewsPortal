using Microsoft.AspNetCore.Mvc;
using ass1Karan.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace ass1Karan.Controllers
{
    public class SavedArticlesController : Controller
    {
        private readonly StudentDBContext _context;

        public SavedArticlesController(StudentDBContext context)
        {
            _context = context;
        }

        
        [HttpPost]
        public async Task<IActionResult> Save(string title, string description, string url, string imageUrl)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null)
            {
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

            TempData["Message"] = "Article saved successfully!";
            return RedirectToAction("Index", "News");
        }

        
        public IActionResult MyArticles()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myArticles = _context.SavedArticles
                .Where(a => a.UserEmail == email)
                .OrderByDescending(a => a.SavedAt)
                .ToList();

            return View(myArticles);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var article = _context.SavedArticles
                .FirstOrDefault(a => a.Id == id && a.UserEmail == userEmail);

            if (article != null)
            {
                _context.SavedArticles.Remove(article);
                _context.SaveChanges();
                TempData["Message"] = "Article deleted successfully!";
            }

            return RedirectToAction("MyArticles");
        }
    }
}
