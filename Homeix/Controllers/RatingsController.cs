using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Homeix.Controllers
{
    [Authorize]
    public class RatingsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public RatingsController(HOMEIXDbContext context)
        {
            _context = context;
        }
        
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var ratings = await _context.Ratings
                .Include(r => r.RatedUser)
                .Include(r => r.RaterUser)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(ratings);
        }
        
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DownloadReport()
        {
            var ratings = await _context.Ratings
                .Include(r => r.RatedUser)
                .Include(r => r.RaterUser)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("RatingId,RatingValue,Review,CreatedAt,RatedUser,RaterUser");

            foreach (var r in ratings)
            {
                sb.AppendLine(
                    $"{r.RatingId}," +
                    $"{r.RatingValue}," +
                    $"\"{r.Review}\"," +
                    $"{r.CreatedAt:yyyy-MM-dd}," +
                    $"\"{r.RatedUser?.FullName}\"," +
                    $"\"{r.RaterUser?.FullName}\""
                );
            }

            return File(
                System.Text.Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                "RatingsReport.csv"
            );
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var rating = await _context.Ratings
                .Include(r => r.RatedUser)
                .Include(r => r.RaterUser)
                .FirstOrDefaultAsync(r => r.RatingId == id);

            if (rating == null) return NotFound();

            return View(rating);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? ratedUserId)
        {
            if (ratedUserId.HasValue)
            {
                var user = await _context.Users.FindAsync(ratedUserId.Value);
                if (user == null) return NotFound();

                ViewBag.RatedUserName = user.FullName;

                return View(new Rating
                {
                    RatedUserId = ratedUserId.Value
                });
            }

            return View(new Rating());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RatedUserId,RatingValue,Review")] Rating rating)
        {
            int raterId = int.Parse(User.FindFirstValue("UserId") ?? "0");

            bool alreadyRated = await _context.Ratings.AnyAsync(r =>
                r.RatedUserId == rating.RatedUserId &&
                r.RaterUserId == raterId);

            if (alreadyRated)
                ModelState.AddModelError("", "You have already rated this user.");

            if (rating.RatingValue < 1 || rating.RatingValue > 5)
                ModelState.AddModelError(nameof(Rating.RatingValue),
                    "Please select a rating between 1 and 5.");

            if (!ModelState.IsValid)
            {
                ViewBag.RatedUserName = (await _context.Users
                    .FindAsync(rating.RatedUserId))?.FullName;

                return View(rating);
            }

            rating.RaterUserId = raterId;
            rating.CreatedAt = DateTime.Now;

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "PublicProfile",
                "Users",
                new { id = rating.RatedUserId }
            );
        }

        [HttpGet]
        [Authorize(Roles = "worker,customer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var rating = await _context.Ratings
                .Include(r => r.RatedUser)
                .FirstOrDefaultAsync(r => r.RatingId == id);

            if (rating == null) return NotFound();

            ViewBag.RatedUserName = rating.RatedUser?.FullName;
            return View(rating);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "worker,customer")]
        public async Task<IActionResult> Edit(int id,[Bind("RatingId,RatedUserId,RatingValue,Review")] Rating rating)
        {
            var existing = await _context.Ratings
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RatingId == id);

            if (existing == null) return NotFound();

            rating.RaterUserId = existing.RaterUserId;
            rating.CreatedAt = existing.CreatedAt;

            if (!ModelState.IsValid)
            {
                ViewBag.RatedUserName = (await _context.Users
                    .FindAsync(rating.RatedUserId))?.FullName;

                return View(rating);
            }

            _context.Update(rating);
            await _context.SaveChangesAsync();

            // ✅ DASHBOARD
            return RedirectToAction("HomeRedirect", "Account");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var rating = await _context.Ratings
                .Include(r => r.RatedUser)
                .FirstOrDefaultAsync(r => r.RatingId == id);

            if (rating == null) return NotFound();

            return View(rating);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null) return NotFound();

            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(Array.Empty<object>());

            var users = await _context.Users
                .Where(u => u.FullName.Contains(term))
                .Select(u => new
                {
                    userId = u.UserId,
                    fullName = u.FullName
                })
                .Take(10)
                .ToListAsync();

            return Json(users);
        }
    }
}
