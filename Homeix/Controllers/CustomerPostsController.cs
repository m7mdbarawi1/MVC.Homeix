using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.CustomerPosts
                .Include(p => p.PostCategory)
                .Include(p => p.User)
                .Include(p => p.Media)
                .FirstOrDefaultAsync(p => p.CustomerPostId == id);

            return post == null ? NotFound() : View(post);
        }

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            LoadDropdowns();
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
            customerPost.CreatedAt = DateTime.Now;

            ModelState.Remove(nameof(CustomerPost.CreatedAt));
            ModelState.Remove(nameof(CustomerPost.UserId));

            if (!ModelState.IsValid)
            {
                LoadDropdowns(customerPost);
                return View(customerPost);
            }

            _context.CustomerPosts.Add(customerPost);
            await _context.SaveChangesAsync();

            if (mediaFiles != null && mediaFiles.Any())
            {
                foreach (var file in mediaFiles.Where(f => f.Length > 0))
                {
                    var path = await SaveCustomerPostMediaAsync(file);

                    _context.CustomerPostMedia.Add(new CustomerPostMedia
                    {
                        CustomerPostId = customerPost.CustomerPostId,
                        MediaPath = path
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
            [Bind("CustomerPostId,PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax")]
            CustomerPost customerPost,
            List<IFormFile>? mediaFiles,
            int[]? deleteMediaIds)
        {
            if (id != customerPost.CustomerPostId)
                return NotFound();

            var existing = await _context.CustomerPosts
                .Include(p => p.Media)
                .FirstOrDefaultAsync(p => p.CustomerPostId == id);

            if (existing == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                LoadDropdowns(customerPost);
                customerPost.Media = existing.Media;
                return View(customerPost);
            }

            // Update fields
            existing.Title = customerPost.Title;
            existing.Description = customerPost.Description;
            existing.Location = customerPost.Location;
            existing.PriceRangeMin = customerPost.PriceRangeMin;
            existing.PriceRangeMax = customerPost.PriceRangeMax;
            existing.PostCategoryId = customerPost.PostCategoryId;

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

            // Add new media (WORKER-PROVEN LOGIC)
            if (mediaFiles != null && mediaFiles.Any())
            {
                foreach (var file in mediaFiles.Where(f => f.Length > 0))
                {
                    var path = await SaveCustomerPostMediaAsync(file);

                    _context.CustomerPostMedia.Add(new CustomerPostMedia
                    {
                        CustomerPostId = id,
                        MediaPath = path
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyPosts));
        }

        // =========================
        // DELETE
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.CustomerPosts
                .Include(p => p.PostCategory)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.CustomerPostId == id);

            return post == null ? NotFound() : View(post);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.CustomerPosts
                .Include(p => p.Media)
                .FirstOrDefaultAsync(p => p.CustomerPostId == id);

            if (post == null)
                return NotFound();

            foreach (var media in post.Media)
            {
                DeletePhysicalFile(media.MediaPath);
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
            var claim = User.FindFirst("UserId")
                ?? throw new UnauthorizedAccessException();

            return int.Parse(claim.Value);
        }

        private void LoadDropdowns(CustomerPost? post = null)
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

        // =========================
        // FILE SAVE (IDENTICAL TO WORKER)
        // =========================
        private static async Task<string> SaveCustomerPostMediaAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext))
                throw new InvalidOperationException("Invalid image type.");

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

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                relativePath.TrimStart('/'));

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }
    }
}
