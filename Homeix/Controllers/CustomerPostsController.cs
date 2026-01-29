using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Controllers
{
    [Authorize]
    public class CustomerPostsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public CustomerPostsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index()
        {
            var posts = await _context.CustomerPosts
                .Include(p => p.PostCategory)
                .Include(p => p.User)
                .Include(p => p.Media)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        // =========================
        // DOWNLOAD REPORT
        // =========================
        public async Task<IActionResult> DownloadReport()
        {
            var posts = await _context.CustomerPosts
                .Include(p => p.PostCategory)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("PostId,Title,Category,User,Location,MinPrice,MaxPrice,Status,IsActive,CreatedAt");

            foreach (var post in posts)
            {
                sb.AppendLine(
                    $"{post.CustomerPostId}," +
                    $"\"{post.Title}\"," +
                    $"\"{post.PostCategory?.CategoryName}\"," +
                    $"\"{post.User?.FullName}\"," +
                    $"\"{post.Location}\"," +
                    $"{post.PriceRangeMin}," +
                    $"{post.PriceRangeMax}," +
                    $"{post.Status}," +
                    $"{post.IsActive}," +
                    $"{post.CreatedAt:yyyy-MM-dd}"
                );
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "CustomerPostsReport.csv");
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.CustomerPosts
                .Include(p => p.PostCategory)
                .Include(p => p.User)
                .Include(p => p.Media) // ✅ new media table
                .FirstOrDefaultAsync(p => p.CustomerPostId == id);

            if (post == null) return NotFound();

            return View(post);
        }

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            LoadDropdowns();
            ViewBag.LoggedInUserId = GetUserId();
            return View();
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax")]
            CustomerPost customerPost,
            List<IFormFile>? mediaFiles)
        {
            customerPost.UserId = GetUserId();
            ModelState.Remove(nameof(CustomerPost.UserId));

            if (!ModelState.IsValid)
            {
                LoadDropdowns(customerPost);
                ViewBag.LoggedInUserId = customerPost.UserId;
                return View(customerPost);
            }

            customerPost.CreatedAt = DateTime.Now;
            customerPost.Status = "Open";
            customerPost.IsActive = true;

            _context.CustomerPosts.Add(customerPost);
            await _context.SaveChangesAsync();

            if (mediaFiles != null && mediaFiles.Any(f => f != null && f.Length > 0))
            {
                foreach (var file in mediaFiles.Where(f => f != null && f.Length > 0))
                {
                    var savedPath = await SaveCustomerPostMediaAsync(file);

                    _context.CustomerPostMedia.Add(new CustomerPostMedia
                    {
                        CustomerPostId = customerPost.CustomerPostId,
                        MediaPath = savedPath,
                        UploadedAt = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyPosts));
        }

        // =========================
        // EDIT (GET)
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.CustomerPosts
                .Include(p => p.Media)
                .FirstOrDefaultAsync(p => p.CustomerPostId == id);

            if (post == null) return NotFound();

            LoadDropdowns(post);
            return View(post);
        }

        // =========================
        // EDIT (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("CustomerPostId,PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax,IsActive")]
            CustomerPost customerPost,
            List<IFormFile>? newMediaFiles,
            int[]? deleteMediaIds)
        {
            if (id != customerPost.CustomerPostId) return NotFound();

            var existing = await _context.CustomerPosts
                .Include(p => p.Media)
                .FirstOrDefaultAsync(p => p.CustomerPostId == id);

            if (existing == null) return NotFound();

            if (!ModelState.IsValid)
            {
                LoadDropdowns(customerPost);
                // show existing media in view
                customerPost.Media = existing.Media;
                return View(customerPost);
            }

            // Keep immutable fields
            customerPost.UserId = existing.UserId;
            customerPost.CreatedAt = existing.CreatedAt;
            customerPost.Status = existing.Status;

            // Update mutable fields
            existing.Title = customerPost.Title;
            existing.Description = customerPost.Description;
            existing.Location = customerPost.Location;
            existing.PriceRangeMin = customerPost.PriceRangeMin;
            existing.PriceRangeMax = customerPost.PriceRangeMax;
            existing.PostCategoryId = customerPost.PostCategoryId;
            existing.IsActive = customerPost.IsActive;

            // Delete selected media
            if (deleteMediaIds != null && deleteMediaIds.Length > 0)
            {
                var toDelete = existing.Media
                    .Where(m => deleteMediaIds.Contains(m.MediaId))
                    .ToList();

                foreach (var media in toDelete)
                {
                    DeletePhysicalFile(media.MediaPath);
                    _context.CustomerPostMedia.Remove(media);
                }
            }

            // Add new media
            if (newMediaFiles != null && newMediaFiles.Any(f => f != null && f.Length > 0))
            {
                foreach (var file in newMediaFiles.Where(f => f != null && f.Length > 0))
                {
                    var savedPath = await SaveCustomerPostMediaAsync(file);

                    _context.CustomerPostMedia.Add(new CustomerPostMedia
                    {
                        CustomerPostId = id,
                        MediaPath = savedPath,
                        UploadedAt = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyPosts));
        }

        // =========================
        // DELETE (GET)
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.CustomerPosts
                .Include(p => p.PostCategory)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.CustomerPostId == id);

            if (post == null) return NotFound();

            return View(post);
        }

        // =========================
        // DELETE (POST)
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.CustomerPosts
                .Include(p => p.Media)
                .FirstOrDefaultAsync(p => p.CustomerPostId == id);

            if (post == null) return NotFound();

            // delete physical files (DB cascade removes rows)
            foreach (var m in post.Media)
            {
                DeletePhysicalFile(m.MediaPath);
            }

            _context.CustomerPosts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyPosts));
        }

        // =========================
        // MY POSTS
        // =========================
        public async Task<IActionResult> MyPosts()
        {
            int userId = GetUserId();

            var posts = await _context.CustomerPosts
                .Where(p => p.UserId == userId)
                .Include(p => p.PostCategory)
                .Include(p => p.Media)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        // =========================
        // HELPERS
        // =========================
        private int GetUserId()
        {
            var claim = User.FindFirst("UserId");
            if (claim == null) throw new UnauthorizedAccessException();
            return int.Parse(claim.Value);
        }

        private void LoadDropdowns(CustomerPost? post = null)
        {
            ViewData["PostCategoryId"] = new SelectList(
                _context.PostCategories.AsNoTracking().OrderBy(c => c.CategoryName),
                "PostCategoryId",
                "CategoryName",
                post?.PostCategoryId
            );
        }

        private static readonly string[] AllowedImageExtensions =
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        };

        private static async Task<string> SaveCustomerPostMediaAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext))
                throw new InvalidOperationException("Only image files are allowed.");

            var uploadDir = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "uploads", "customer-posts");

            Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadDir, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/uploads/customer-posts/" + fileName;
        }

        private static void DeletePhysicalFile(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            var physicalPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                relativePath.TrimStart('/'));

            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);
        }
    }
}
