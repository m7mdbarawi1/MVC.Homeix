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

        // ================= ADMIN =================
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            // ===== USERS BY ROLE =====
            var usersByRole = await _context.Users
                .Include(u => u.Role)
                .GroupBy(u => u.Role.RoleName)
                .Select(g => new
                {
                    Role = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            ViewBag.UsersByRoleLabels =
                JsonSerializer.Serialize(usersByRole.Select(x => x.Role).ToList());

            ViewBag.UsersByRoleCounts =
                JsonSerializer.Serialize(usersByRole.Select(x => x.Count).ToList());

            // ===== POSTS =====
            ViewBag.CustomerPostsCount = await _context.CustomerPosts.CountAsync();
            ViewBag.WorkerPostsCount = await _context.WorkerPosts.CountAsync();

            // ===== JOB STATUS =====
            var jobStatuses = await _context.JobProgresses
                .GroupBy(j => j.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            ViewBag.JobStatusLabels =
                JsonSerializer.Serialize(jobStatuses.Select(x => x.Status).ToList());

            ViewBag.JobStatusCounts =
                JsonSerializer.Serialize(jobStatuses.Select(x => x.Count).ToList());

            // ===== PAYMENTS (LAST 6 MONTHS) =====
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

            ViewBag.PaymentMonths =
                JsonSerializer.Serialize(
                    payments.Select(p =>
                        CultureInfo.CurrentCulture
                            .DateTimeFormat
                            .GetAbbreviatedMonthName(p.Month))
                    .ToList());

            ViewBag.PaymentTotals =
                JsonSerializer.Serialize(payments.Select(p => p.Total).ToList());

            // ===== RATINGS =====
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
                JsonSerializer.Serialize(
                    ratings.Select(r => $"⭐ {r.Rating}").ToList());

            ViewBag.RatingCounts =
                JsonSerializer.Serialize(ratings.Select(r => r.Count).ToList());

            return View();
        }

        // ================= CUSTOMER =================
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> CustomerDashboard()
        {
            var workerPosts = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.User)
                    .ThenInclude(u => u.RatingRatedUsers)   // ⭐ ratings
                .Include(w => w.PostMedia)                  // 🖼 images (THIS WAS MISSING)
                .Where(w => w.IsActive)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(workerPosts);
        }

        // ================= WORKER =================
        [Authorize(Roles = "worker")]
        public async Task<IActionResult> WorkerDashboard()
        {
            var customerPosts = await _context.CustomerPosts
                .Include(c => c.PostCategory)
                .Include(c => c.User)
                 // ⭐ REQUIRED
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(customerPosts);
        }
    }
}
