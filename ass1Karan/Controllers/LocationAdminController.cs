using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ass1Karan.Models;
using ass1Karan.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ass1Karan.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LocationAdminController : Controller
    {
        private readonly StudentDBContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ActivityLogService _activityLogService;

        public LocationAdminController(StudentDBContext context, IWebHostEnvironment env, ActivityLogService activityLogService)
        {
            _context = context;
            _env = env;
            _activityLogService = activityLogService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var list = await _context.LocationEntries
                    .OrderBy(x => x.Tag)
                    .ToListAsync();

                return View(list);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading entries.";
                Console.WriteLine($"[LocationAdmin Index Error]: {ex.Message}");
                return View(new List<LocationEntry>());
            }
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(LocationEntry model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Invalid input.";
                    return View(model);
                }

                _context.LocationEntries.Add(model);
                await _context.SaveChangesAsync();

                await _activityLogService.LogAsync("ADMIN", $"Created Location Entry '{model.Title}'");

                TempData["Message"] = "Entry created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error creating entry.";
                Console.WriteLine($"[LocationAdmin Create Error]: {ex.Message}");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var entry = await _context.LocationEntries.FindAsync(id);
            return entry == null ? NotFound() : View(entry);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(LocationEntry model)
        {
            try
            {
                var entry = await _context.LocationEntries.FindAsync(model.Id);
                if (entry == null)
                {
                    TempData["ErrorMessage"] = "Entry not found.";
                    return RedirectToAction("Index");
                }

                entry.Title = model.Title;
                entry.Address = model.Address;
                entry.Phone = model.Phone;
                entry.DirectionsUrl = model.DirectionsUrl;
                entry.Tag = model.Tag;

                _context.LocationEntries.Update(entry);
                await _context.SaveChangesAsync();

                await _activityLogService.LogAsync("ADMIN", $"Updated Location Entry '{entry.Title}'");

                TempData["Message"] = "Entry updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating entry.";
                Console.WriteLine($"[LocationAdmin Edit Error]: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var entry = await _context.LocationEntries.FindAsync(id);
            return entry == null ? NotFound() : View(entry);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var entry = await _context.LocationEntries.FindAsync(id);
                if (entry == null)
                {
                    TempData["ErrorMessage"] = "Entry not found.";
                    return RedirectToAction("Index");
                }

                _context.LocationEntries.Remove(entry);
                await _context.SaveChangesAsync();

                await _activityLogService.LogAsync("ADMIN", $"Deleted Location Entry '{entry.Title}'");

                TempData["Message"] = "Entry deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting entry.";
                Console.WriteLine($"[LocationAdmin Delete Error]: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
        public IActionResult TagImages()
        {
            try
            {
                var list = _context.TagImages.ToList();
                return View(list);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading tag images.";
                Console.WriteLine($"[LocationAdmin TagImage Error]: {ex.Message}");
                return View(new List<TagImage>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadTagImage(string tag, IFormFile imageFile)
        {
            try
            {
                if (imageFile == null || string.IsNullOrEmpty(tag))
                {
                    TempData["ErrorMessage"] = "Select Tag and Image.";
                    return RedirectToAction("TagImages");
                }

                string folder = Path.Combine(_env.WebRootPath, "uploads/tagImages");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                string path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                    await imageFile.CopyToAsync(stream);

                var existing = _context.TagImages.FirstOrDefault(x => x.Tag == tag);

                if (existing == null)
                {
                    existing = new TagImage { Tag = tag, ImageName = fileName };
                    _context.TagImages.Add(existing);
                }
                else
                {
                    existing.ImageName = fileName;
                    _context.TagImages.Update(existing);
                }

                await _context.SaveChangesAsync();
                await _activityLogService.LogAsync("ADMIN", $"Updated Tag Image for '{tag}'");

                TempData["Message"] = "Tag image updated.";
                return RedirectToAction("TagImages");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error uploading tag image.";
                Console.WriteLine($"[LocationAdmin UploadTagImage Error]: {ex.Message}");
                return RedirectToAction("TagImages");
            }
        }
    }
}
