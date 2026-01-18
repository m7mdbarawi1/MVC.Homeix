using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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

        // =====================================================
        // ADMIN: VIEW ALL WORKER POSTS
        // =====================================================
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

        // =====================================================
        // DOWNLOAD REPORT (CSV)
        // =====================================================
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DownloadReport()
        {
            var posts = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.User)
                    .ThenInclude(u => u.RatingRatedUsers)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("PostId,Title,Category,Worker,Location,MinPrice,MaxPrice,AvgRating,TotalRatings,IsActive,CreatedAt");

            foreach (var post in posts)
            {
                var ratings = post.User?.RatingRatedUsers ?? Enumerable.Empty<Rating>();
                var avgRating = ratings.Any() ? ratings.Average(r => r.RatingValue) : 0;

                sb.AppendLine(
                    $"{post.WorkerPostId}," +
                    $"\"{post.Title}\"," +
                    $"\"{post.PostCategory?.CategoryName}\"," +
                    $"\"{post.User?.FullName}\"," +
                    $"\"{post.Location}\"," +
                    $"{post.PriceRangeMin}," +
                    $"{post.PriceRangeMax}," +
                    $"{avgRating:0.0}," +
                    $"{ratings.Count()}," +
                    $"{post.IsActive}," +
                    $"{post.CreatedAt:yyyy-MM-dd}"
                );
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "WorkerPostsReport.csv");
        }

        // =====================================================
        // DETAILS
        // =====================================================
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.PostMedia)
                .Include(w => w.User)
                    .ThenInclude(u => u.RatingRatedUsers)
                .FirstOrDefaultAsync(w => w.WorkerPostId == id);

            if (post == null)
                return NotFound();

            return View(post);
        }

        // =====================================================
        // CREATE (GET)
        // =====================================================
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

        // =====================================================
        // CREATE (POST)
        // =====================================================
        [HttpPost]
        [Authorize(Roles = "worker, admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkerPost workerPost)
        {
            int userId = GetUserId();

            await DebugSubscriptions(userId);

            if (User.IsInRole("worker"))
            {
                var subscription = await GetCurrentSubscriptionAsync(userId);

                if (subscription == null ||
                    subscription.StartDate > DateTime.Today ||
                    subscription.EndDate < DateTime.Today)
                {
                    ModelState.AddModelError("", "Your subscription is not currently valid.");
                    LoadCategories(workerPost);
                    return View(workerPost);
                }

                var windowStart = DateTime.Now.AddDays(-30);

                int postsThisMonth = await _context.WorkerPosts.CountAsync(w =>
                    w.UserId == userId &&
                    w.CreatedAt >= windowStart);

                if (subscription.Plan?.MaxPostsPerMonth.HasValue == true &&
                    postsThisMonth >= subscription.Plan.MaxPostsPerMonth.Value)
                {
                    TempData["Error"] =
                        $"You have reached your limit of {subscription.Plan.MaxPostsPerMonth} posts.";

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

            return RedirectToAction(nameof(MyPosts));
        }

        // =====================================================
        // MY POSTS
        // =====================================================
        [Authorize(Roles = "worker")]
        public async Task<IActionResult> MyPosts()
        {
            int userId = GetUserId();

            var posts = await _context.WorkerPosts
                .Where(w => w.UserId == userId)
                .Include(w => w.PostCategory)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        // =====================================================
        // DELETE
        // =====================================================
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

            _context.WorkerPosts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyPosts));
        }

        // =====================================================
        // HELPERS
        // =====================================================
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
    }
}
