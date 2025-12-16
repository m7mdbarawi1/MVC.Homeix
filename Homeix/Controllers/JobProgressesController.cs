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
    public class JobProgressesController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public JobProgressesController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: JobProgresses
        // ========================
        public async Task<IActionResult> Index()
        {
            var jobs = await _context.JobProgresses
                .Include(j => j.AssignedToUser)
                .Include(j => j.CustomerPost)
                .Include(j => j.RequestedByUser)
                .ToListAsync();

            return View(jobs);
        }

        // ========================
        // GET: JobProgresses/Details/5
        // ========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var jobProgress = await _context.JobProgresses
                .Include(j => j.AssignedToUser)
                .Include(j => j.CustomerPost)
                .Include(j => j.RequestedByUser)
                .FirstOrDefaultAsync(j => j.JobProgressId == id);

            if (jobProgress == null)
                return NotFound();

            return View(jobProgress);
        }

        // ========================
        // GET: JobProgresses/Create
        // ========================
        public IActionResult Create()
        {
            ReloadDropdowns();
            return View();
        }

        // ========================
        // POST: JobProgresses/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "CustomerPostId," +
                "RequestedByUserId," +
                "AssignedToUserId"
            )]
            JobProgress jobProgress)
        {
            if (!ModelState.IsValid)
            {
                ReloadDropdowns(jobProgress);
                return View(jobProgress);
            }

            // =========================
            // System-managed fields
            // =========================
            jobProgress.Status = "In Progress";
            jobProgress.StartedAt = DateTime.Now;
            jobProgress.IsRatedByCustomer = false;
            jobProgress.IsRatedByWorker = false;

            _context.JobProgresses.Add(jobProgress);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: JobProgresses/Edit
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var jobProgress = await _context.JobProgresses.FindAsync(id);
            if (jobProgress == null)
                return NotFound();

            ReloadDropdowns(jobProgress);
            return View(jobProgress);
        }

        // ========================
        // POST: JobProgresses/Edit
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "JobProgressId," +
                "CustomerPostId," +
                "RequestedByUserId," +
                "AssignedToUserId," +
                "Status," +
                "CompletedAt," +
                "IsRatedByCustomer," +
                "IsRatedByWorker"
            )]
            JobProgress jobProgress)
        {
            if (id != jobProgress.JobProgressId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ReloadDropdowns(jobProgress);
                return View(jobProgress);
            }

            var existing = await _context.JobProgresses
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.JobProgressId == id);

            if (existing == null)
                return NotFound();

            // =========================
            // Preserve system fields
            // =========================
            jobProgress.StartedAt = existing.StartedAt;

            _context.Update(jobProgress);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: JobProgresses/Delete
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var jobProgress = await _context.JobProgresses
                .Include(j => j.AssignedToUser)
                .Include(j => j.CustomerPost)
                .Include(j => j.RequestedByUser)
                .FirstOrDefaultAsync(j => j.JobProgressId == id);

            if (jobProgress == null)
                return NotFound();

            return View(jobProgress);
        }

        // ========================
        // POST: JobProgresses/Delete
        // ========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var jobProgress = await _context.JobProgresses.FindAsync(id);
            if (jobProgress != null)
            {
                _context.JobProgresses.Remove(jobProgress);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // Helpers
        // ========================
        private void ReloadDropdowns(JobProgress? job = null)
        {
            ViewData["CustomerPostId"] = new SelectList(
                _context.CustomerPosts,
                "CustomerPostId",
                "CustomerPostId",
                job?.CustomerPostId
            );

            ViewData["RequestedByUserId"] = new SelectList(
                _context.Users,
                "UserId",
                "UserId",
                job?.RequestedByUserId
            );

            ViewData["AssignedToUserId"] = new SelectList(
                _context.Users,
                "UserId",
                "UserId",
                job?.AssignedToUserId
            );
        }
    }
}
