using ass1Karan.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

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
            var user = await _user.GetUserAsync(User);

            if (user == null)
                return Json(new { status = "error", message = "User not logged in" });

            string email = user.Email;

            var existing = await _db.PpsResults
                .FirstOrDefaultAsync(x => x.UserEmail == email && x.Diagnosis == data.Diagnosis);

            if (existing != null)
            {
                existing.Ambulation = data.Ambulation;
                existing.Activity = data.Activity;
                existing.Evidence = data.Evidence;
                existing.SelfCare = data.SelfCare;
                existing.Intake = data.Intake;
                existing.Consciousness = data.Consciousness;
                existing.FinalScore = data.FinalScore;
                existing.DateSaved = DateTime.Now;

                await _db.SaveChangesAsync();
                return Json(new { status = "updated" });
            }

            data.UserEmail = email;
            data.DateSaved = DateTime.Now;

            _db.PpsResults.Add(data);
            await _db.SaveChangesAsync();

            return Json(new { status = "created" });
        }

    }
}
