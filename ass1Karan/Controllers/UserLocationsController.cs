using ass1Karan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ass1Karan.Controllers
{
    public class UserLocationsController : Controller
    {
        private readonly StudentDBContext _context;

        public UserLocationsController(StudentDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string tag = "All", int page = 1)
        {
            try
            {
                int pageSize = 4;

                var query = _context.LocationEntries.AsQueryable();

                if (tag != "All")
                    query = query.Where(x => x.Tag == tag);

                var totalItems = await query.CountAsync();
                var locations = await query
                    .OrderBy(x => x.Title)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var tagImage = _context.TagImages.FirstOrDefault(x => x.Tag == tag);

                ViewBag.Tag = tag;
                ViewBag.Page = page;
                ViewBag.TotalPages = (int)System.Math.Ceiling((double)totalItems / pageSize);
                ViewBag.TagImageName = tagImage?.ImageName;

                return View(locations);
            }
            catch
            {
                ViewBag.Tag = tag;
                ViewBag.Page = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TagImageName = null;
                return View(new List<LocationEntry>());
            }
        }
    }
}
