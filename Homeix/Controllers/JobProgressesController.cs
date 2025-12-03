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
    public class JobProgressesController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public JobProgressesController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // GET: JobProgresses
        public async Task<IActionResult> Index()
        {
            var hOMEIXDbContext = _context.JobProgresses.Include(j => j.AssignedToUser).Include(j => j.CustomerPost).Include(j => j.RequestedByUser);
            return View(await hOMEIXDbContext.ToListAsync());
        }

        // GET: JobProgresses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jobProgress = await _context.JobProgresses
                .Include(j => j.AssignedToUser)
                .Include(j => j.CustomerPost)
                .Include(j => j.RequestedByUser)
                .FirstOrDefaultAsync(m => m.JobProgressId == id);
            if (jobProgress == null)
            {
                return NotFound();
            }

            return View(jobProgress);
        }

        // GET: JobProgresses/Create
        public IActionResult Create()
        {
            ViewData["AssignedToUserId"] = new SelectList(_context.Users, "UserId", "UserId");
            ViewData["CustomerPostId"] = new SelectList(_context.CustomerPosts, "CustomerPostId", "CustomerPostId");
            ViewData["RequestedByUserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: JobProgresses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("JobProgressId,CustomerPostId,RequestedByUserId,AssignedToUserId,Status,StartedAt,CompletedAt,IsRatedByCustomer,IsRatedByWorker")] JobProgress jobProgress)
        {
            if (ModelState.IsValid)
            {
                _context.Add(jobProgress);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AssignedToUserId"] = new SelectList(_context.Users, "UserId", "UserId", jobProgress.AssignedToUserId);
            ViewData["CustomerPostId"] = new SelectList(_context.CustomerPosts, "CustomerPostId", "CustomerPostId", jobProgress.CustomerPostId);
            ViewData["RequestedByUserId"] = new SelectList(_context.Users, "UserId", "UserId", jobProgress.RequestedByUserId);
            return View(jobProgress);
        }

        // GET: JobProgresses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jobProgress = await _context.JobProgresses.FindAsync(id);
            if (jobProgress == null)
            {
                return NotFound();
            }
            ViewData["AssignedToUserId"] = new SelectList(_context.Users, "UserId", "UserId", jobProgress.AssignedToUserId);
            ViewData["CustomerPostId"] = new SelectList(_context.CustomerPosts, "CustomerPostId", "CustomerPostId", jobProgress.CustomerPostId);
            ViewData["RequestedByUserId"] = new SelectList(_context.Users, "UserId", "UserId", jobProgress.RequestedByUserId);
            return View(jobProgress);
        }

        // POST: JobProgresses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("JobProgressId,CustomerPostId,RequestedByUserId,AssignedToUserId,Status,StartedAt,CompletedAt,IsRatedByCustomer,IsRatedByWorker")] JobProgress jobProgress)
        {
            if (id != jobProgress.JobProgressId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(jobProgress);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobProgressExists(jobProgress.JobProgressId))
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
            ViewData["AssignedToUserId"] = new SelectList(_context.Users, "UserId", "UserId", jobProgress.AssignedToUserId);
            ViewData["CustomerPostId"] = new SelectList(_context.CustomerPosts, "CustomerPostId", "CustomerPostId", jobProgress.CustomerPostId);
            ViewData["RequestedByUserId"] = new SelectList(_context.Users, "UserId", "UserId", jobProgress.RequestedByUserId);
            return View(jobProgress);
        }

        // GET: JobProgresses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jobProgress = await _context.JobProgresses
                .Include(j => j.AssignedToUser)
                .Include(j => j.CustomerPost)
                .Include(j => j.RequestedByUser)
                .FirstOrDefaultAsync(m => m.JobProgressId == id);
            if (jobProgress == null)
            {
                return NotFound();
            }

            return View(jobProgress);
        }

        // POST: JobProgresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var jobProgress = await _context.JobProgresses.FindAsync(id);
            if (jobProgress != null)
            {
                _context.JobProgresses.Remove(jobProgress);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JobProgressExists(int id)
        {
            return _context.JobProgresses.Any(e => e.JobProgressId == id);
        }
    }
}
