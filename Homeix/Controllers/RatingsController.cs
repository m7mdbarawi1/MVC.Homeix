using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;

namespace Homeix.Controllers
{
    public class RatingsController : Controller
    {
        private readonly HOMEIXDbContext _context;
        public RatingsController(HOMEIXDbContext context) { _context = context;}
        public async Task<IActionResult> Index()
        {
            var ratings = await _context.Ratings.Include(r => r.RatedUser).Include(r => r.RaterUser).OrderByDescending(r => r.CreatedAt).ToListAsync();
            return View(ratings);
        }
        public async Task<IActionResult> DownloadReport()
        {
            var ratings = await _context.Ratings.Include(r => r.RatedUser).Include(r => r.RaterUser).OrderByDescending(r => r.CreatedAt).ToListAsync();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("RatingId,RatingValue,Review,CreatedAt,RatedUser,RaterUser");
            foreach (var r in ratings)
            {
                sb.AppendLine($"{r.RatingId}," + $"{r.RatingValue}," + $"\"{r.Review}\"," + $"{r.CreatedAt:yyyy-MM-dd}," + $"\"{r.RatedUser?.FullName}\"," + $"\"{r.RaterUser?.FullName}\"");
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "RatingsReport.csv");
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var rating = await _context.Ratings.Include(r => r.RatedUser).Include(r => r.RaterUser).FirstOrDefaultAsync(r => r.RatingId == id);
            if (rating == null) return NotFound();
            return View(rating);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RatedUserId,RatingValue,Review")] Rating rating)
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            int loggedUserId = int.Parse(userIdClaim);
            if (rating.RatedUserId == loggedUserId) ModelState.AddModelError("", "You cannot rate yourself.");
            if (rating.RatedUserId <= 0) ModelState.AddModelError(nameof(Rating.RatedUserId), "Please select a user to rate.");
            if (rating.RatingValue < 1 || rating.RatingValue > 5) ModelState.AddModelError(nameof(Rating.RatingValue), "Please select a rating.");
            if (!ModelState.IsValid) return View(rating);
            rating.RaterUserId = loggedUserId;
            rating.CreatedAt = DateTime.Now;
            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null) return NotFound();
            return View(rating);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RatingId,RatedUserId,RatingValue,Review")] Rating rating)
        {
            if (id != rating.RatingId) return NotFound();
            var existing = await _context.Ratings.AsNoTracking().FirstOrDefaultAsync(r => r.RatingId == id);
            if (existing == null) return NotFound();
            if (rating.RatedUserId == existing.RaterUserId) ModelState.AddModelError("", "You cannot rate yourself.");
            if (!ModelState.IsValid) return View(rating);
            rating.RaterUserId = existing.RaterUserId;
            rating.CreatedAt = existing.CreatedAt;
            _context.Update(rating);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var rating = await _context.Ratings.Include(r => r.RatedUser).Include(r => r.RaterUser).FirstOrDefaultAsync(r => r.RatingId == id);
            if (rating == null) return NotFound();
            return View(rating);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rating = await _context.Ratings.FindAsync(id);
            if (rating != null)
            {
                _context.Ratings.Remove(rating);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return Json(Array.Empty<object>());
            var users = await _context.Users.Where(u => u.FullName.Contains(term)).Select(u => new {userId = u.UserId,fullName = u.FullName}).Take(10).ToListAsync();
            return Json(users);
        }
    }
}
