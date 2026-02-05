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

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var usersByRole = await _context.Users
                .Include(u => u.Role)
                .GroupBy(u => u.Role!.RoleName)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.UsersByRoleLabels =
                JsonSerializer.Serialize(usersByRole.Select(x => x.Role));

            ViewBag.UsersByRoleCounts =
                JsonSerializer.Serialize(usersByRole.Select(x => x.Count));

            ViewBag.CustomerPostsCount = await _context.CustomerPosts.CountAsync();
            ViewBag.WorkerPostsCount = await _context.WorkerPosts.CountAsync();

            var payments = await _context.Payments
                .Where(p => p.PaymentDate >= DateTime.Now.AddMonths(-6))
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new
                {
                    g.Key.Month,
                    Total = g.Sum(x => x.Amount)
                })
                .OrderBy(g => g.Month)
                .ToListAsync();

            ViewBag.PaymentMonths = JsonSerializer.Serialize(
                payments.Select(p =>
                    CultureInfo.CurrentCulture
                        .DateTimeFormat
                        .GetAbbreviatedMonthName(p.Month)
                )
            );

            ViewBag.PaymentTotals =
                JsonSerializer.Serialize(payments.Select(p => p.Total));

            var ratings = await _context.Ratings
                .GroupBy(r => r.RatingValue)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .OrderBy(g => g.Rating)
                .ToListAsync();

            ViewBag.RatingLabels =
                JsonSerializer.Serialize(ratings.Select(r => $"⭐ {r.Rating}"));

            ViewBag.RatingCounts =
                JsonSerializer.Serialize(ratings.Select(r => r.Count));

            return View();
        }

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

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst("UserId");

            if (claim == null || !int.TryParse(claim.Value, out int userId))
                throw new InvalidOperationException("Invalid UserId claim.");

            return userId;
        }
    }
}
