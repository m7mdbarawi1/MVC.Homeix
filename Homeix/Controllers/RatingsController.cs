using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;

namespace Homeix.Controllers
{
    public class RatingsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public RatingsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: Ratings
        // ========================
        public async Task<IActionResult> Index()
        {
            var ratings = await _context.Ratings
                .Include(r => r.JobProgress)
                .Include(r => r.RatedUser)
                .Include(r => r.RaterUser)
                .ToListAsync();

            return View(ratings);
        }

        // ========================
        // GET: Ratings/Details
        // ========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var rating = await _context.Ratings
                .Include(r => r.JobProgress)
                .Include(r => r.RatedUser)
                .Include(r => r.RaterUser)
                .FirstOrDefaultAsync(m => m.RatingId == id);

            if (rating == null)
                return NotFound();

            return View(rating);
        }

        // ========================
        // GET: Ratings/Create
        // ========================
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // ========================
        // POST: Ratings/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("JobProgressId,RaterUserId,RatedUserId,RatingValue,Review")]
            Rating rating)
        {
            if (rating.RaterUserId == rating.RatedUserId)
            {
                ModelState.AddModelError("", "You cannot rate yourself.");
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns(rating);
                return View(rating);
            }

            // =========================
            // SYSTEM FIELD
            // =========================
            rating.CreatedAt = DateTime.UtcNow;

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: Ratings/Edit
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null)
                return NotFound();

            LoadDropdowns(rating);
            return View(rating);
        }

        // ========================
        // POST: Ratings/Edit
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("RatingId,JobProgressId,RaterUserId,RatedUserId,RatingValue,Review")]
            Rating rating)
        {
            if (id != rating.RatingId)
                return NotFound();

            if (rating.RaterUserId == rating.RatedUserId)
            {
                ModelState.AddModelError("", "You cannot rate yourself.");
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns(rating);
                return View(rating);
            }

            var existing = await _context.Ratings
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RatingId == id);

            if (existing == null)
                return NotFound();

            // Preserve system field
            rating.CreatedAt = existing.CreatedAt;

            _context.Update(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: Ratings/Delete
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var rating = await _context.Ratings
                .Include(r => r.JobProgress)
                .Include(r => r.RatedUser)
                .Include(r => r.RaterUser)
                .FirstOrDefaultAsync(m => m.RatingId == id);

            if (rating == null)
                return NotFound();

            return View(rating);
        }

        // ========================
        // POST: Ratings/Delete
        // ========================
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

        // ========================
        // Helpers
        // ========================
        private void LoadDropdowns(Rating? rating = null)
        {
            ViewData["JobProgressId"] =
                new SelectList(
                    _context.JobProgresses,
                    "JobProgressId",
                    "JobProgressId",
                    rating?.JobProgressId);

            ViewData["RatedUserId"] =
                new SelectList(
                    _context.Users,
                    "UserId",
                    "FullName",   // ✅ UX FIX
                    rating?.RatedUserId);

            ViewData["RaterUserId"] =
                new SelectList(
                    _context.Users,
                    "UserId",
                    "FullName",   // ✅ UX FIX
                    rating?.RaterUserId);
        }
    }
}
