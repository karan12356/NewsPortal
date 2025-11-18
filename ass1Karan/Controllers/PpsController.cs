using Microsoft.AspNetCore.Mvc;
using ass1Karan.Models;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ass1Karan.Controllers
{
    [Authorize]
    public class PpsController : Controller
    {
        private readonly StudentDBContext _db;
        private readonly UserManager<IdentityUser> _user;

        public PpsController(StudentDBContext db, UserManager<IdentityUser> user)
        {
            _db = db;
            _user = user;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveResult([FromBody] PpsResult data)
        {
            data.UserEmail = (await _user.GetUserAsync(User)).Email;
            data.DateSaved = DateTime.Now;
            _db.PpsResults.Add(data);
            await _db.SaveChangesAsync();
            return Json(new { status = "ok" });
        }
    }
}
