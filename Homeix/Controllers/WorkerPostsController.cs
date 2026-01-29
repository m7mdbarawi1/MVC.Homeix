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
using Microsoft.Extensions.Logging;

namespace Homeix.Controllers
{
    [Authorize]
    public class WorkerPostsController : Controller
    {
        private readonly HOMEIXDbContext _context;
        private readonly ILogger<WorkerPostsController> _logger;

        public WorkerPostsController(
            HOMEIXDbContext context,
            ILogger<WorkerPostsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =========================
        // INDEX (ADMIN)
        // =========================
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var posts = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.User)
                    .ThenInclude(u => u.RatingRatedUsers)
                .Where(w => w.IsActive)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.Media)
                .Include(w => w.User)
                    .ThenInclude(u => u.RatingRatedUsers)
                .FirstOrDefaultAsync(w => w.WorkerPostId == id);

            if (post == null) return NotFound();
            return View(post);
        }

        // =========================
        // CREATE (GET)
        // =========================
        [Authorize(Roles = "worker,admin")]
        public async Task<IActionResult> Create()
        {
            LoadCategories();
            return View();
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [Authorize(Roles = "worker,admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            WorkerPost workerPost,
            List<IFormFile>? mediaFiles)
        {
            int userId = GetUserId();

            if (!ModelState.IsValid)
            {
                LoadCategories(workerPost);
                return View(workerPost);
            }

            workerPost.UserId = userId;
            workerPost.CreatedAt = DateTime.Now;
            workerPost.IsActive = true;

            _context.WorkerPosts.Add(workerPost);
            await _context.SaveChangesAsync();

            if (mediaFiles != null && mediaFiles.Any())
            {
                foreach (var file in mediaFiles.Where(f => f.Length > 0))
                {
                    var path = await SaveWorkerPostMediaAsync(file);

                    _context.WorkerPostMedia.Add(new WorkerPostMedia
                    {
                        WorkerPostId = workerPost.WorkerPostId,
                        MediaPath = path,
                        UploadedAt = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyPosts));
        }

        // =========================
        // MY POSTS
        // =========================
        [Authorize(Roles = "worker")]
        public async Task<IActionResult> MyPosts()
        {
            int userId = GetUserId();

            var posts = await _context.WorkerPosts
                .Where(w => w.UserId == userId)
                .Include(w => w.PostCategory)
                .Include(w => w.Media)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        // =========================
        // EDIT (GET)
        // =========================
        [Authorize(Roles = "worker,admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.Media)
                .FirstOrDefaultAsync(w => w.WorkerPostId == id);

            if (post == null) return NotFound();
            if (post.UserId != GetUserId() && !User.IsInRole("admin"))
                return Forbid();

            LoadCategories(post);
            return View(post);
        }

        // =========================
        // EDIT (POST)
        // =========================
        [HttpPost]
        [Authorize(Roles = "worker,admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            WorkerPost workerPost,
            List<IFormFile>? newMediaFiles,
            int[]? deleteMediaIds)
        {
            var existing = await _context.WorkerPosts
                .Include(w => w.Media)
                .FirstOrDefaultAsync(w => w.WorkerPostId == id);

            if (existing == null) return NotFound();
            if (existing.UserId != GetUserId() && !User.IsInRole("admin"))
                return Forbid();

            if (!ModelState.IsValid)
            {
                LoadCategories(workerPost);
                return View(workerPost);
            }

            // Update fields
            existing.Title = workerPost.Title;
            existing.Description = workerPost.Description;
            existing.Location = workerPost.Location;
            existing.PriceRangeMin = workerPost.PriceRangeMin;
            existing.PriceRangeMax = workerPost.PriceRangeMax;
            existing.PostCategoryId = workerPost.PostCategoryId;
            existing.IsActive = workerPost.IsActive;

            // Delete selected media
            if (deleteMediaIds != null && deleteMediaIds.Length > 0)
            {
                var toDelete = existing.Media
                    .Where(m => deleteMediaIds.Contains(m.MediaId))
                    .ToList();

                foreach (var media in toDelete)
                {
                    DeletePhysicalFile(media.MediaPath);
                    _context.WorkerPostMedia.Remove(media);
                }
            }

            // Add new media
            if (newMediaFiles != null)
            {
                foreach (var file in newMediaFiles.Where(f => f.Length > 0))
                {
                    var path = await SaveWorkerPostMediaAsync(file);

                    _context.WorkerPostMedia.Add(new WorkerPostMedia
                    {
                        WorkerPostId = id,
                        MediaPath = path,
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
        [Authorize(Roles = "worker,admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .FirstOrDefaultAsync(w => w.WorkerPostId == id);

            if (post == null) return NotFound();
            if (post.UserId != GetUserId() && !User.IsInRole("admin"))
                return Forbid();

            return View(post);
        }

        // =========================
        // DELETE (POST)
        // =========================
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "worker,admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.WorkerPosts
                .Include(w => w.Media)
                .FirstOrDefaultAsync(w => w.WorkerPostId == id);

            if (post == null) return NotFound();
            if (post.UserId != GetUserId() && !User.IsInRole("admin"))
                return Forbid();

            foreach (var media in post.Media)
            {
                DeletePhysicalFile(media.MediaPath);
            }

            _context.WorkerPosts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyPosts));
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

        private void LoadCategories(WorkerPost? post = null)
        {
            ViewData["PostCategoryId"] = new SelectList(
                _context.PostCategories.AsNoTracking(),
                "PostCategoryId",
                "CategoryName",
                post?.PostCategoryId
            );
        }

        private static readonly string[] AllowedImageExtensions =
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        };

        private static async Task<string> SaveWorkerPostMediaAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext))
                throw new InvalidOperationException("Invalid image type.");

            var uploadDir = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "uploads", "worker-posts");

            Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadDir, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/uploads/worker-posts/" + fileName;
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
