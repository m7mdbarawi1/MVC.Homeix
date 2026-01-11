using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Homeix.Controllers
{
    [Authorize]
    public class AdvertisementsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public AdvertisementsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: Advertisements
        // ========================
        public async Task<IActionResult> Index()
        {
            var ads = await _context.Advertisements
                .Include(a => a.CreatedByUser)
                .ToListAsync();

            return View(ads);
        }

        // ========================
        // GET: Advertisements/Create
        // ========================
        public IActionResult Create()
        {
            return View();
        }

        // ========================
        // POST: Advertisements/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Advertisement advertisement)
        {
            ModelState.Remove(nameof(Advertisement.ImagePath));
            ModelState.Remove(nameof(Advertisement.CreatedByUserId));

            if (advertisement.ImageFile == null || advertisement.ImageFile.Length == 0)
                ModelState.AddModelError(nameof(advertisement.ImageFile), "Please upload an image.");

            if (!ModelState.IsValid)
                return View(advertisement);

            // ✅ Auto assign logged-in user
            advertisement.CreatedByUserId =
                int.Parse(User.FindFirstValue("UserId")!);

            // ✅ Validate extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(advertisement.ImageFile!.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(advertisement.ImageFile), "Only image files are allowed.");
                return View(advertisement);
            }

            // ✅ Save file
            var uploadPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "uploads", "advertisements"
            );
            Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await advertisement.ImageFile.CopyToAsync(stream);
            }

            advertisement.ImagePath = "/uploads/advertisements/" + fileName;

            _context.Advertisements.Add(advertisement);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: Advertisements/Delete/5
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var advertisement = await _context.Advertisements
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.AdId == id);

            if (advertisement == null)
                return NotFound();

            return View(advertisement);
        }

        // ========================
        // POST: Advertisements/Delete
        // ========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);

            if (advertisement != null)
            {
                // delete image file
                if (!string.IsNullOrEmpty(advertisement.ImagePath))
                {
                    var physicalPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        advertisement.ImagePath.TrimStart('/')
                    );

                    if (System.IO.File.Exists(physicalPath))
                        System.IO.File.Delete(physicalPath);
                }

                _context.Advertisements.Remove(advertisement);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
