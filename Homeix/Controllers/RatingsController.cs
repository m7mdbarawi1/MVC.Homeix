using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Models;
using Homeix.Data;

namespace Homeix.Controllers
{
    public class RatingsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public RatingsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // GET: Ratings
        public async Task<IActionResult> Index()
        {
            var hOMEIXDbContext = _context.Ratings.Include(r => r.JobProgress).Include(r => r.RatedUser).Include(r => r.RaterUser);
            return View(await hOMEIXDbContext.ToListAsync());
        }

        // GET: Ratings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rating = await _context.Ratings
                .Include(r => r.JobProgress)
                .Include(r => r.RatedUser)
                .Include(r => r.RaterUser)
                .FirstOrDefaultAsync(m => m.RatingId == id);
            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }

        // GET: Ratings/Create
        public IActionResult Create()
        {
            ViewData["JobProgressId"] = new SelectList(_context.JobProgresses, "JobProgressId", "JobProgressId");
            ViewData["RatedUserId"] = new SelectList(_context.Users, "UserId", "UserId");
            ViewData["RaterUserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Ratings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RatingId,JobProgressId,RaterUserId,RatedUserId,RatingValue,Review,CreatedAt")] Rating rating)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rating);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["JobProgressId"] = new SelectList(_context.JobProgresses, "JobProgressId", "JobProgressId", rating.JobProgressId);
            ViewData["RatedUserId"] = new SelectList(_context.Users, "UserId", "UserId", rating.RatedUserId);
            ViewData["RaterUserId"] = new SelectList(_context.Users, "UserId", "UserId", rating.RaterUserId);
            return View(rating);
        }

        // GET: Ratings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null)
            {
                return NotFound();
            }
            ViewData["JobProgressId"] = new SelectList(_context.JobProgresses, "JobProgressId", "JobProgressId", rating.JobProgressId);
            ViewData["RatedUserId"] = new SelectList(_context.Users, "UserId", "UserId", rating.RatedUserId);
            ViewData["RaterUserId"] = new SelectList(_context.Users, "UserId", "UserId", rating.RaterUserId);
            return View(rating);
        }

        // POST: Ratings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RatingId,JobProgressId,RaterUserId,RatedUserId,RatingValue,Review,CreatedAt")] Rating rating)
        {
            if (id != rating.RatingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rating);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RatingExists(rating.RatingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["JobProgressId"] = new SelectList(_context.JobProgresses, "JobProgressId", "JobProgressId", rating.JobProgressId);
            ViewData["RatedUserId"] = new SelectList(_context.Users, "UserId", "UserId", rating.RatedUserId);
            ViewData["RaterUserId"] = new SelectList(_context.Users, "UserId", "UserId", rating.RaterUserId);
            return View(rating);
        }

        // GET: Ratings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rating = await _context.Ratings
                .Include(r => r.JobProgress)
                .Include(r => r.RatedUser)
                .Include(r => r.RaterUser)
                .FirstOrDefaultAsync(m => m.RatingId == id);
            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }

        // POST: Ratings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rating = await _context.Ratings.FindAsync(id);
            if (rating != null)
            {
                _context.Ratings.Remove(rating);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RatingExists(int id)
        {
            return _context.Ratings.Any(e => e.RatingId == id);
        }
    }
}
