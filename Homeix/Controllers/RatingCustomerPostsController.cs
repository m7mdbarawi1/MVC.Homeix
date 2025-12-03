using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        // GET: RatingCustomerPosts
        public async Task<IActionResult> Index()
        {
            var hOMEIXDbContext = _context.RatingCustomerPosts.Include(r => r.JobProgress).Include(r => r.RatedUser).Include(r => r.RaterUser);
            return View(await hOMEIXDbContext.ToListAsync());
        }

        // GET: RatingCustomerPosts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ratingCustomerPost = await _context.RatingCustomerPosts
                .Include(r => r.JobProgress)
                .Include(r => r.RatedUser)
                .Include(r => r.RaterUser)
                .FirstOrDefaultAsync(m => m.RatingCustomerPostId == id);
            if (ratingCustomerPost == null)
            {
                return NotFound();
            }

            return View(ratingCustomerPost);
        }

        // GET: RatingCustomerPosts/Create
        public IActionResult Create()
        {
            ViewData["JobProgressId"] = new SelectList(_context.JobProgresses, "JobProgressId", "JobProgressId");
            ViewData["RatedUserId"] = new SelectList(_context.Users, "UserId", "UserId");
            ViewData["RaterUserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: RatingCustomerPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RatingCustomerPostId,JobProgressId,RaterUserId,RatedUserId,RatingValue,Review,CreatedAt")] RatingCustomerPost ratingCustomerPost)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ratingCustomerPost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["JobProgressId"] = new SelectList(_context.JobProgresses, "JobProgressId", "JobProgressId", ratingCustomerPost.JobProgressId);
            ViewData["RatedUserId"] = new SelectList(_context.Users, "UserId", "UserId", ratingCustomerPost.RatedUserId);
            ViewData["RaterUserId"] = new SelectList(_context.Users, "UserId", "UserId", ratingCustomerPost.RaterUserId);
            return View(ratingCustomerPost);
        }

        // GET: RatingCustomerPosts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ratingCustomerPost = await _context.RatingCustomerPosts.FindAsync(id);
            if (ratingCustomerPost == null)
            {
                return NotFound();
            }
            ViewData["JobProgressId"] = new SelectList(_context.JobProgresses, "JobProgressId", "JobProgressId", ratingCustomerPost.JobProgressId);
            ViewData["RatedUserId"] = new SelectList(_context.Users, "UserId", "UserId", ratingCustomerPost.RatedUserId);
            ViewData["RaterUserId"] = new SelectList(_context.Users, "UserId", "UserId", ratingCustomerPost.RaterUserId);
            return View(ratingCustomerPost);
        }

        // POST: RatingCustomerPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RatingCustomerPostId,JobProgressId,RaterUserId,RatedUserId,RatingValue,Review,CreatedAt")] RatingCustomerPost ratingCustomerPost)
        {
            if (id != ratingCustomerPost.RatingCustomerPostId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ratingCustomerPost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RatingCustomerPostExists(ratingCustomerPost.RatingCustomerPostId))
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
            ViewData["JobProgressId"] = new SelectList(_context.JobProgresses, "JobProgressId", "JobProgressId", ratingCustomerPost.JobProgressId);
            ViewData["RatedUserId"] = new SelectList(_context.Users, "UserId", "UserId", ratingCustomerPost.RatedUserId);
            ViewData["RaterUserId"] = new SelectList(_context.Users, "UserId", "UserId", ratingCustomerPost.RaterUserId);
            return View(ratingCustomerPost);
        }

        // GET: RatingCustomerPosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ratingCustomerPost = await _context.RatingCustomerPosts
                .Include(r => r.JobProgress)
                .Include(r => r.RatedUser)
                .Include(r => r.RaterUser)
                .FirstOrDefaultAsync(m => m.RatingCustomerPostId == id);
            if (ratingCustomerPost == null)
            {
                return NotFound();
            }

            return View(ratingCustomerPost);
        }

        // POST: RatingCustomerPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ratingCustomerPost = await _context.RatingCustomerPosts.FindAsync(id);
            if (ratingCustomerPost != null)
            {
                _context.RatingCustomerPosts.Remove(ratingCustomerPost);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RatingCustomerPostExists(int id)
        {
            return _context.RatingCustomerPosts.Any(e => e.RatingCustomerPostId == id);
        }
    }
}
