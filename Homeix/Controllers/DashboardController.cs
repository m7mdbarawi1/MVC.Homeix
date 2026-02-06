using Homeix.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace Homeix.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public DashboardController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================= ADMIN DASHBOARD =========================
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            // -------- USERS BY ROLE --------
            var usersByRole = await _context.Users
                .Include(u => u.Role)
                .GroupBy(u => u.Role!.RoleName)
                .Select(g => new
                {
                    Role = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            ViewBag.UsersByRoleLabels =
                JsonSerializer.Serialize(usersByRole.Select(x => x.Role));

            ViewBag.UsersByRoleCounts =
                JsonSerializer.Serialize(usersByRole.Select(x => x.Count));

            // -------- POSTS DISTRIBUTION --------
            ViewBag.CustomerPostsCount = await _context.CustomerPosts.CountAsync();
            ViewBag.WorkerPostsCount = await _context.WorkerPosts.CountAsync();

            // -------- DAILY REVENUE (LAST 30 DAYS) --------
            var payments = await _context.Payments
                .Where(p => p.PaymentDate.Date >= DateTime.Today.AddDays(-30))
                .GroupBy(p => p.PaymentDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(x => x.Amount)
                })
                .OrderBy(g => g.Date)
                .ToListAsync();

            ViewBag.PaymentDays = JsonSerializer.Serialize(
                payments.Select(p =>
                    p.Date.ToString("dd MMM", CultureInfo.CurrentCulture)
                )
            );

            ViewBag.PaymentTotals =
                JsonSerializer.Serialize(payments.Select(p => p.Total));

            // -------- RATINGS DISTRIBUTION --------
            var ratings = await _context.Ratings
                .GroupBy(r => r.RatingValue)
                .Select(g => new
                {
                    Rating = g.Key,
                    Count = g.Count()
                })
                .OrderBy(g => g.Rating)
                .ToListAsync();

            ViewBag.RatingLabels =
                JsonSerializer.Serialize(ratings.Select(r => $"⭐ {r.Rating}"));

            ViewBag.RatingCounts =
                JsonSerializer.Serialize(ratings.Select(r => r.Count));

            return View();
        }

        // ========================= CUSTOMER DASHBOARD =========================
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> CustomerDashboard()
        {
            int userId = GetCurrentUserId();

            var workerPosts = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.Media)
                .Include(w => w.User)
                    .ThenInclude(u => u.RatingRatedUsers)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            var favoriteIds = await _context.FavoriteWorkerPosts
                .Where(f => f.UserId == userId)
                .Select(f => f.WorkerPostId)
                .ToListAsync();

            ViewBag.FavoritePostIds = favoriteIds;

            return View(workerPosts);
        }

        // ========================= WORKER DASHBOARD =========================
        [Authorize(Roles = "worker")]
        public async Task<IActionResult> WorkerDashboard()
        {
            int userId = GetCurrentUserId();

            var customerPosts = await _context.CustomerPosts
                .Include(c => c.PostCategory)
                .Include(c => c.Media)
                .Include(c => c.User)
                    .ThenInclude(u => u.RatingRatedUsers)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var favoriteIds = await _context.FavoriteCustomerPosts
                .Where(f => f.UserId == userId)
                .Select(f => f.CustomerPostId)
                .ToListAsync();

            ViewBag.FavoritePostIds = favoriteIds;

            return View(customerPosts);
        }

        // ========================= HELPERS =========================
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst("UserId");

            if (claim == null || !int.TryParse(claim.Value, out int userId))
                throw new InvalidOperationException("Invalid UserId claim.");

            return userId;
        }
    }
}
