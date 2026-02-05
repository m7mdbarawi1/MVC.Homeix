using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // ================= INDEX =================
        public async Task<IActionResult> Index()
        {
            var ads = await _context.Advertisements
                .Include(a => a.CreatedByUser)
                .ToListAsync();

            return View(ads);
        }

        // ================= DOWNLOAD REPORT =================
        public async Task<IActionResult> DownloadReport()
        {
            var ads = await _context.Advertisements
                .Include(a => a.CreatedByUser)
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("AdId,Title,ImagePath,StartDate,EndDate,IsActive,CreatedBy");

            foreach (var a in ads)
            {
                sb.AppendLine(
                    $"{a.AdId}," +
                    $"\"{a.Title}\"," +
                    $"\"{a.ImagePath}\"," +
                    $"{a.StartDate:yyyy-MM-dd}," +
                    $"{a.EndDate:yyyy-MM-dd}," +
                    $"{a.IsActive}," +
                    $"\"{a.CreatedByUser?.FullName}\""
                );
            }

            return File(
                Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                "AdvertisementsReport.csv"
            );
        }

        // ================= DETAILS =================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ad = await _context.Advertisements
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.AdId == id);

            if (ad == null) return NotFound();
            return View(ad);
        }

        // ================= CREATE (GET) =================
        public IActionResult Create()
        {
            return View();
        }

        // ================= CREATE (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Advertisement advertisement)
        {
            ModelState.Remove(nameof(Advertisement.ImagePath));
            ModelState.Remove(nameof(Advertisement.CreatedByUserId));

            if (advertisement.ImageFile == null || advertisement.ImageFile.Length == 0)
            {
                ModelState.AddModelError(nameof(advertisement.ImageFile),
                    "Please upload an image.");
            }

            if (!ModelState.IsValid)
                return View(advertisement);

            advertisement.CreatedByUserId =
                int.Parse(User.FindFirstValue("UserId")!);

            advertisement.ImagePath =
                await SaveAdvertisementImageAsync(advertisement.ImageFile!);

            _context.Advertisements.Add(advertisement);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT (GET) =================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ad = await _context.Advertisements.FindAsync(id);
            if (ad == null) return NotFound();

            return View(ad);
        }

        // ================= EDIT (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Advertisement advertisement,
            IFormFile? ImageFile)
        {
            if (id != advertisement.AdId)
                return NotFound();

            var existing = await _context.Advertisements
                .FirstOrDefaultAsync(a => a.AdId == id);

            if (existing == null)
                return NotFound();

            ModelState.Remove(nameof(Advertisement.ImagePath));
            ModelState.Remove(nameof(Advertisement.CreatedByUserId));

            if (!ModelState.IsValid)
                return View(advertisement);

            // UPDATE FIELDS
            existing.Title = advertisement.Title;
            existing.StartDate = advertisement.StartDate;
            existing.EndDate = advertisement.EndDate;
            existing.IsActive = advertisement.IsActive;

            // 🔥 IMAGE UPDATE (MATCHES WORKER POST LOGIC)
            if (ImageFile != null && ImageFile.Length > 0)
            {
                DeletePhysicalFile(existing.ImagePath);
                existing.ImagePath = await SaveAdvertisementImageAsync(ImageFile);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ================= DELETE (GET) =================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ad = await _context.Advertisements
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.AdId == id);

            if (ad == null) return NotFound();
            return View(ad);
        }

        // ================= DELETE (POST) =================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ad = await _context.Advertisements.FindAsync(id);

            if (ad != null)
            {
                DeletePhysicalFile(ad.ImagePath);
                _context.Advertisements.Remove(ad);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ================= IMAGE HELPERS =================

        private static readonly string[] AllowedImageExtensions =
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        };

        private static async Task<string> SaveAdvertisementImageAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext))
                throw new InvalidOperationException("Invalid image type.");

            var uploadDir = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "uploads", "advertisements");

            Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadDir, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/uploads/advertisements/" + fileName;
        }

        private static void DeletePhysicalFile(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                relativePath.TrimStart('/'));

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }
    }
}
