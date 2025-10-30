using Microsoft.AspNetCore.Mvc;
using ass1Karan.Services;
using Microsoft.AspNetCore.Http;
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
           
            var email = HttpContext.Session.GetString("UserEmail");
            

            var articles = await _newsService.GetTopHeadlinesAsync();
            return View(articles);
        }

        [HttpPost]
        public IActionResult SaveArticle(string title, string url)
        {
            
            TempData["Message"] = $"Article '{title}' saved!";
            return RedirectToAction("Index");
        }
    }
}
