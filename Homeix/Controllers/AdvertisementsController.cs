using System;
using System.IO;
using System.Linq;
using System.Text;
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
            sb.AppendLine("AdId,Title,ImagePath,StartDate,EndDate,IsActive,CreatedByUserId");

            foreach (var a in ads)
            {
                sb.AppendLine(
                    $"{a.AdId}," +
                    $"\"{a.Title}\"," +
                    $"{a.ImagePath}," +
                    $"{a.StartDate:yyyy-MM-dd}," +
                    $"{a.EndDate:yyyy-MM-dd}," +
                    $"{a.IsActive}," +
                    $"{a.CreatedByUserId}"
                );
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "AdvertisementsReport.csv");
        }

        // ================= DETAILS =================
        public async Task<IActionResult> Details(int? id)
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
                ModelState.AddModelError(nameof(advertisement.ImageFile), "Please upload an image.");
            }

            if (!ModelState.IsValid)
                return View(advertisement);

            advertisement.CreatedByUserId =
                int.Parse(User.FindFirstValue("UserId")!);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(advertisement.ImageFile!.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(advertisement.ImageFile), "Only image files are allowed.");
                return View(advertisement);
            }

            var uploadPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "advertisements"
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

        // ================= EDIT (GET) =================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var advertisement = await _context.Advertisements.FindAsync(id);

            if (advertisement == null)
                return NotFound();

            return View(advertisement);
        }

        // ================= EDIT (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Advertisement advertisement)
        {
            if (id != advertisement.AdId)
                return NotFound();

            ModelState.Remove(nameof(Advertisement.ImagePath));
            ModelState.Remove(nameof(Advertisement.CreatedByUserId));

            if (!ModelState.IsValid)
                return View(advertisement);

            try
            {
                _context.Update(advertisement);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Advertisements.Any(e => e.AdId == advertisement.AdId))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ================= DELETE (GET) =================
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

        // ================= DELETE (POST) =================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);

            if (advertisement != null)
            {
                if (!string.IsNullOrWhiteSpace(advertisement.ImagePath))
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
