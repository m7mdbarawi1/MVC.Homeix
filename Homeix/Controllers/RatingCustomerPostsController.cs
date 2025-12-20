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
    public class RatingCustomerPostsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public RatingCustomerPostsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: RatingCustomerPosts
        // ========================
        public async Task<IActionResult> Index()
        {
            var ratings = await _context.RatingCustomerPosts
                .Include(r => r.JobProgress)
                .Include(r => r.RatedUser)
                .Include(r => r.RaterUser)
                .ToListAsync();

            return View(ratings);
        }

        // ========================
        // GET: RatingCustomerPosts/Details
        // ========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var rating = await _context.RatingCustomerPosts
                .Include(r => r.JobProgress)
                .Include(r => r.RatedUser)
                .Include(r => r.RaterUser)
                .FirstOrDefaultAsync(m => m.RatingCustomerPostId == id);

            if (rating == null)
                return NotFound();

            return View(rating);
        }

        // ========================
        // GET: RatingCustomerPosts/Create
        // ========================
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // ========================
        // POST: RatingCustomerPosts/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("JobProgressId,RaterUserId,RatedUserId,RatingValue,Review")]
            RatingCustomerPost rating)
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

            _context.RatingCustomerPosts.Add(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: RatingCustomerPosts/Edit
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var rating = await _context.RatingCustomerPosts.FindAsync(id);
            if (rating == null)
                return NotFound();

            LoadDropdowns(rating);
            return View(rating);
        }

        // ========================
        // POST: RatingCustomerPosts/Edit
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("RatingCustomerPostId,JobProgressId,RaterUserId,RatedUserId,RatingValue,Review")]
            RatingCustomerPost rating)
        {
            if (id != rating.RatingCustomerPostId)
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

            var existing = await _context.RatingCustomerPosts
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RatingCustomerPostId == id);

            if (existing == null)
                return NotFound();

            // Preserve system field
            rating.CreatedAt = existing.CreatedAt;

            _context.Update(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: RatingCustomerPosts/Delete
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var rating = await _context.RatingCustomerPosts
                .Include(r => r.JobProgress)
                .Include(r => r.RatedUser)
                .Include(r => r.RaterUser)
                .FirstOrDefaultAsync(m => m.RatingCustomerPostId == id);

            if (rating == null)
                return NotFound();

            return View(rating);
        }

        // ========================
        // POST: RatingCustomerPosts/Delete
        // ========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rating = await _context.RatingCustomerPosts.FindAsync(id);
            if (rating != null)
            {
                _context.RatingCustomerPosts.Remove(rating);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // Helpers
        // ========================
        private void LoadDropdowns(RatingCustomerPost? rating = null)
        {
            ViewData["JobProgressId"] =
                new SelectList(_context.JobProgresses,
                    "JobProgressId",
                    "JobProgressId",
                    rating?.JobProgressId);

            ViewData["RatedUserId"] =
                new SelectList(_context.Users,
                    "UserId",
                    "FullName",   // ✅ UX
                    rating?.RatedUserId);

            ViewData["RaterUserId"] =
                new SelectList(_context.Users,
                    "UserId",
                    "FullName",   // ✅ UX
                    rating?.RaterUserId);
        }
    }
}
