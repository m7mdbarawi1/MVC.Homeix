using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;

namespace Homeix.Controllers
{
    public class PostMediumsController : Controller
    {
        private readonly HOMEIXDbContext _context;
        public PostMediumsController(HOMEIXDbContext context) { _context = context;}
        public async Task<IActionResult> Index() { return View(await _context.PostMedia.ToListAsync());}
        public async Task<IActionResult> DownloadReport()
        {
            var media = await _context.PostMedia.OrderByDescending(m => m.UploadedAt).ToListAsync();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("MediaId,PostType,PostId,MediaPath,UploadedAt");
            foreach (var m in media)
            {
                sb.AppendLine($"{m.MediaId}," + $"{m.PostType}," + $"{m.PostId}," + $"\"{m.MediaPath}\"," + $"{m.UploadedAt:yyyy-MM-dd HH:mm}");
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "PostMediaReport.csv");
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var postMedium = await _context.PostMedia.FirstOrDefaultAsync(m => m.MediaId == id);
            if (postMedium == null) return NotFound();
            return View(postMedium);
        }
        public IActionResult Create() {return View();}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostMedium postMedium)
        {
            ModelState.Remove(nameof(PostMedium.MediaPath));
            ModelState.Remove(nameof(PostMedium.UploadedAt));
            if (postMedium.MediaFile == null || postMedium.MediaFile.Length == 0) ModelState.AddModelError(nameof(PostMedium.MediaFile), "Please upload an image or video.");
            if (!ModelState.IsValid) return View(postMedium);
            var allowedExtensions = new[] {".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov", ".webm"};
            var extension = Path.GetExtension(postMedium.MediaFile!.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(PostMedium.MediaFile), "Only images or videos are allowed.");
                return View(postMedium);
            }
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "post-media");
            Directory.CreateDirectory(uploadPath);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create)) {await postMedium.MediaFile.CopyToAsync(stream);}
            postMedium.MediaPath = "/uploads/post-media/" + fileName;
            postMedium.UploadedAt = DateTime.Now;
            _context.PostMedia.Add(postMedium);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var postMedium = await _context.PostMedia.FindAsync(id);
            if (postMedium == null) return NotFound();
            return View(postMedium);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PostMedium postMedium)
        {
            if (id != postMedium.MediaId) return NotFound();
            ModelState.Remove(nameof(PostMedium.MediaPath));
            ModelState.Remove(nameof(PostMedium.UploadedAt));
            if (!ModelState.IsValid) return View(postMedium);
            var existing = await _context.PostMedia.FindAsync(id);
            if (existing == null) return NotFound();
            existing.PostType = postMedium.PostType;
            existing.PostId = postMedium.PostId;
            if (postMedium.MediaFile != null && postMedium.MediaFile.Length > 0)
            {
                var extension = Path.GetExtension(postMedium.MediaFile.FileName).ToLowerInvariant();
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "post-media");
                var fileName = $"{Guid.NewGuid()}{extension}";
                var fullPath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create)) {await postMedium.MediaFile.CopyToAsync(stream);}
                if (!string.IsNullOrEmpty(existing.MediaPath))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existing.MediaPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }
                existing.MediaPath = "/uploads/post-media/" + fileName;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var postMedium = await _context.PostMedia.FirstOrDefaultAsync(m => m.MediaId == id);
            if (postMedium == null) return NotFound();
            return View(postMedium);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var postMedium = await _context.PostMedia.FindAsync(id);
            if (postMedium != null)
            {
                if (!string.IsNullOrEmpty(postMedium.MediaPath))
                {
                    var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", postMedium.MediaPath.TrimStart('/'));
                    if (System.IO.File.Exists(physicalPath)) System.IO.File.Delete(physicalPath);
                }
                _context.PostMedia.Remove(postMedium);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
