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
        public WorkerPostsController(HOMEIXDbContext context, ILogger<WorkerPostsController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var posts = await _context.WorkerPosts.Include(w => w.PostCategory).Include(w => w.User).ThenInclude(u => u.RatingRatedUsers).Where(w => w.IsActive).OrderByDescending(w => w.CreatedAt).ToListAsync();
            return View(posts);
        }
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DownloadReport()
        {
            var posts = await _context.WorkerPosts.Include(w => w.PostCategory).Include(w => w.User).ThenInclude(u => u.RatingRatedUsers).OrderByDescending(w => w.CreatedAt).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("PostId,Title,Category,Worker,Location,MinPrice,MaxPrice,AvgRating,TotalRatings,IsActive,CreatedAt");
            foreach (var post in posts)
            {
                var ratings = post.User?.RatingRatedUsers ?? Enumerable.Empty<Rating>();
                var avgRating = ratings.Any() ? ratings.Average(r => r.RatingValue) : 0;
                sb.AppendLine($"{post.WorkerPostId}," + $"\"{post.Title}\"," + $"\"{post.PostCategory?.CategoryName}\"," + $"\"{post.User?.FullName}\"," + $"\"{post.Location}\"," + $"{post.PriceRangeMin}," + $"{post.PriceRangeMax}," + $"{avgRating:0.0}," + $"{ratings.Count()}," + $"{post.IsActive}," + $"{post.CreatedAt:yyyy-MM-dd}");
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "WorkerPostsReport.csv");
        }
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.WorkerPosts.Include(w => w.PostCategory).Include(w => w.PostMedia).Include(w => w.User).ThenInclude(u => u.RatingRatedUsers).FirstOrDefaultAsync(w => w.WorkerPostId == id);
            if (post == null) return NotFound();
            post.PostMedia = await _context.PostMedia.Where(m => m.PostType == "WorkerPost" && m.PostId == id).OrderByDescending(m => m.UploadedAt).ToListAsync();
            return View(post);
        }
        [Authorize(Roles = "worker, admin")]
        public async Task<IActionResult> Create()
        {
            int userId = GetUserId();
            if (User.IsInRole("worker"))
            {
                var subscription = await GetCurrentSubscriptionAsync(userId);
                if (subscription == null)
                {
                    TempData["Error"] = "You must have an active subscription to create posts.";
                    return RedirectToAction("Index", "SubscriptionPlans");
                }
            }
            LoadCategories();
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "worker, admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkerPost workerPost, List<IFormFile>? mediaFiles)
        {
            int userId = GetUserId();
            await DebugSubscriptions(userId);
            if (User.IsInRole("worker"))
            {
                var subscription = await GetCurrentSubscriptionAsync(userId);
                if (subscription == null || subscription.StartDate > DateTime.Today || subscription.EndDate < DateTime.Today)
                {
                    ModelState.AddModelError("", "Your subscription is not currently valid.");
                    LoadCategories(workerPost);
                    return View(workerPost);
                }
                var windowStart = DateTime.Now.AddDays(-30);
                int postsThisMonth = await _context.WorkerPosts.CountAsync(w =>w.UserId == userId && w.CreatedAt >= windowStart);
                if (subscription.Plan?.MaxPostsPerMonth.HasValue == true && postsThisMonth >= subscription.Plan.MaxPostsPerMonth.Value)
                {
                    TempData["Error"] = $"You have reached your limit of {subscription.Plan.MaxPostsPerMonth} posts.";
                    return RedirectToAction("Index", "SubscriptionPlans");
                }
            }
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
            var postedFiles = Request.Form.Files.Where(f => f.Name == "mediaFiles" && f.Length > 0).ToList();
            if (postedFiles.Count == 0 && mediaFiles != null) postedFiles = mediaFiles.Where(f => f != null && f.Length > 0).ToList();
            if (postedFiles.Count > 0)
            {
                foreach (var file in postedFiles)
                {
                    var savedPath = await SaveWorkerPostMediaAsync(file);
                    _context.PostMedia.Add(new PostMedium { PostType = "WorkerPost", PostId = workerPost.WorkerPostId, MediaPath = savedPath, UploadedAt = DateTime.Now });
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(MyPosts));
        }
        [Authorize(Roles = "worker")]
        public async Task<IActionResult> MyPosts()
        {
            int userId = GetUserId();
            var posts = await _context.WorkerPosts.Where(w => w.UserId == userId).Include(w => w.PostCategory).OrderByDescending(w => w.CreatedAt).ToListAsync();
            var postIds = posts.Select(p => p.WorkerPostId).ToList();
            var allMedia = await _context.PostMedia.Where(m => m.PostType == "WorkerPost" && postIds.Contains(m.PostId)).OrderByDescending(m => m.UploadedAt).ToListAsync();
            foreach (var p in posts) { p.PostMedia = allMedia.Where(m => m.PostId == p.WorkerPostId && m.PostType == "WorkerPost").OrderByDescending(m => m.UploadedAt).ToList();}
            return View(posts);
        }
        [Authorize(Roles = "worker, admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .FirstOrDefaultAsync(w => w.WorkerPostId == id);

            if (post == null) return NotFound();

            if (post.UserId != GetUserId() && !User.IsInRole("admin"))
                return Forbid();

            post.PostMedia = await _context.PostMedia
                .Where(m => m.PostType == "WorkerPost" && m.PostId == post.WorkerPostId)
                .OrderByDescending(m => m.UploadedAt)
                .ToListAsync();

            LoadCategories(post);

            if (User.IsInRole("admin"))
            {
                ViewData["UserId"] = new SelectList(
                    _context.Users.AsNoTracking()
                        .OrderBy(u => u.FullName),
                    "UserId",
                    "FullName",
                    post.UserId
                );
            }

            return View(post);
        }
        [HttpPost]
        [Authorize(Roles = "worker, admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            WorkerPost workerPost,
            List<IFormFile>? newMediaFiles,
            int[]? deleteMediaIds)
        {
            if (id != workerPost.WorkerPostId)
                return NotFound();

            var existing = await _context.WorkerPosts
                .FirstOrDefaultAsync(w => w.WorkerPostId == id);

            if (existing == null)
                return NotFound();

            if (existing.UserId != GetUserId() && !User.IsInRole("admin"))
                return Forbid();

            if (!User.IsInRole("admin"))
                workerPost.UserId = existing.UserId;

            if (!ModelState.IsValid)
            {
                workerPost.PostMedia = await _context.PostMedia
                    .Where(m => m.PostType == "WorkerPost" && m.PostId == id)
                    .OrderByDescending(m => m.UploadedAt)
                    .ToListAsync();

                LoadCategories(workerPost);

                if (User.IsInRole("admin"))
                {
                    ViewData["UserId"] = new SelectList(
                        _context.Users.AsNoTracking().OrderBy(u => u.FullName),
                        "UserId",
                        "FullName",
                        workerPost.UserId
                    );
                }

                return View(workerPost);
            }

            existing.Title = workerPost.Title;
            existing.Description = workerPost.Description;
            existing.Location = workerPost.Location;
            existing.PriceRangeMin = workerPost.PriceRangeMin;
            existing.PriceRangeMax = workerPost.PriceRangeMax;
            existing.PostCategoryId = workerPost.PostCategoryId;
            existing.IsActive = workerPost.IsActive;

            if (User.IsInRole("admin"))
            {
                existing.UserId = workerPost.UserId;
                existing.CreatedAt = workerPost.CreatedAt;
            }

            if (deleteMediaIds != null && deleteMediaIds.Length > 0)
            {
                var mediaToDelete = await _context.PostMedia
                    .Where(m => deleteMediaIds.Contains(m.MediaId)
                                && m.PostType == "WorkerPost"
                                && m.PostId == id)
                    .ToListAsync();

                foreach (var media in mediaToDelete)
                {
                    DeletePhysicalFile(media.MediaPath);
                    _context.PostMedia.Remove(media);
                }
            }

            if (newMediaFiles != null && newMediaFiles.Any(f => f != null && f.Length > 0))
            {
                foreach (var file in newMediaFiles.Where(f => f != null && f.Length > 0))
                {
                    var savedPath = await SaveWorkerPostMediaAsync(file);

                    _context.PostMedia.Add(new PostMedium
                    {
                        PostType = "WorkerPost",
                        PostId = id,
                        MediaPath = savedPath,
                        UploadedAt = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyPosts));
        }
        [Authorize(Roles = "worker, admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .FirstOrDefaultAsync(w => w.WorkerPostId == id);

            if (post == null) return NotFound();

            if (post.UserId != GetUserId() && !User.IsInRole("admin"))
                return Forbid();

            return View(post);
        }
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "worker, admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.WorkerPosts.FindAsync(id);
            if (post == null) return NotFound();

            if (post.UserId != GetUserId() && !User.IsInRole("admin"))
                return Forbid();

            var media = await _context.PostMedia
                .Where(m => m.PostType == "WorkerPost" && m.PostId == id)
                .ToListAsync();

            foreach (var m in media)
            {
                DeletePhysicalFile(m.MediaPath);
                _context.PostMedia.Remove(m);
            }

            _context.WorkerPosts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyPosts));
        }
        private int GetUserId()
        {
            var claim = User.FindFirst("UserId");
            if (claim == null)
                throw new UnauthorizedAccessException();

            return int.Parse(claim.Value);
        }
        private void LoadCategories(WorkerPost? post = null)
        {
            ViewData["PostCategoryId"] = new SelectList(
                _context.PostCategories.AsNoTracking().OrderBy(c => c.CategoryName),
                "PostCategoryId",
                "CategoryName",
                post?.PostCategoryId
            );
        }
        private async Task<Subscription?> GetCurrentSubscriptionAsync(int userId)
        {
            return await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId && s.Status.Trim().ToLower() == "active")
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();
        }
        private async Task DebugSubscriptions(int userId)
        {
            var subs = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.EndDate)
                .ToListAsync();

            foreach (var s in subs)
            {
                _logger.LogInformation(
                    "ID={Id} | Status={Status} | Start={Start} | End={End} | Plan={Plan}",
                    s.SubscriptionId,
                    s.Status,
                    s.StartDate,
                    s.EndDate,
                    s.Plan?.PlanName
                );
            }
        }
        private static readonly string[] AllowedImageExtensions =
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        };
        private async Task<string> SaveWorkerPostMediaAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedImageExtensions.Contains(extension))
                throw new InvalidOperationException("Only image files are allowed.");

            var uploadPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "uploads", "post-media"
            );

            Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/uploads/post-media/" + fileName;
        }
        private void DeletePhysicalFile(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            var physicalPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                relativePath.TrimStart('/')
            );

            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);
        }
    }
}