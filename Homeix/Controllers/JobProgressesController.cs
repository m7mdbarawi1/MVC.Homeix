using System;
using System.Linq;
using System.Text;
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
        public JobProgressesController(HOMEIXDbContext context){_context = context;}
        public async Task<IActionResult> Index()
        {
            var jobs = await _context.JobProgresses.Include(j => j.AssignedToUser).Include(j => j.CustomerPost).Include(j => j.RequestedByUser).ToListAsync();
            return View(jobs);
        }
        public async Task<IActionResult> DownloadReport()
        {
            var jobs = await _context.JobProgresses.Include(j => j.AssignedToUser).Include(j => j.CustomerPost).Include(j => j.RequestedByUser).OrderByDescending(j => j.StartedAt).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("JobProgressId,Status,StartedAt,CompletedAt,IsRatedByCustomer,IsRatedByWorker,AssignedToUserId,CustomerPostId,RequestedByUserId");
            foreach (var j in jobs)
            {
                sb.AppendLine($"{j.JobProgressId}," + $"{j.Status}," + $"{j.StartedAt:yyyy-MM-dd HH:mm}," + $"{j.CompletedAt?.ToString("yyyy-MM-dd HH:mm")}," + $"{j.IsRatedByCustomer}," + $"{j.IsRatedByWorker}," + $"{j.AssignedToUserId}," + $"{j.CustomerPostId}," + $"{j.RequestedByUserId}");
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "JobProgressReport.csv");
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var jobProgress = await _context.JobProgresses.Include(j => j.AssignedToUser).Include(j => j.CustomerPost).Include(j => j.RequestedByUser).FirstOrDefaultAsync(j => j.JobProgressId == id);
            if (jobProgress == null) return NotFound();
            return View(jobProgress);
        }
        public IActionResult Create()
        {
            ReloadDropdowns();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerPostId,RequestedByUserId,AssignedToUserId")] JobProgress jobProgress)
        {
            if (!ModelState.IsValid)
            {
                ReloadDropdowns(jobProgress);
                return View(jobProgress);
            }
            jobProgress.Status = "In Progress";
            jobProgress.StartedAt = DateTime.Now;
            jobProgress.IsRatedByCustomer = false;
            jobProgress.IsRatedByWorker = false;
            _context.JobProgresses.Add(jobProgress);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var jobProgress = await _context.JobProgresses.FindAsync(id);
            if (jobProgress == null) return NotFound();
            ReloadDropdowns(jobProgress);
            return View(jobProgress);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("JobProgressId,CustomerPostId,RequestedByUserId,AssignedToUserId,Status,CompletedAt,IsRatedByCustomer,IsRatedByWorker")]JobProgress jobProgress)
        {
            if (id != jobProgress.JobProgressId) return NotFound();
            if (!ModelState.IsValid)
            {
                ReloadDropdowns(jobProgress);
                return View(jobProgress);
            }
            var existing = await _context.JobProgresses.AsNoTracking().FirstOrDefaultAsync(j => j.JobProgressId == id);
            if (existing == null) return NotFound();
            jobProgress.StartedAt = existing.StartedAt;
            _context.Update(jobProgress);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var jobProgress = await _context.JobProgresses.Include(j => j.AssignedToUser).Include(j => j.CustomerPost).Include(j => j.RequestedByUser).FirstOrDefaultAsync(j => j.JobProgressId == id);
            if (jobProgress == null) return NotFound();
            return View(jobProgress);
        }
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
        private void ReloadDropdowns(JobProgress? job = null)
        {
            ViewData["CustomerPostId"] = new SelectList(_context.CustomerPosts, "CustomerPostId", "CustomerPostId", job?.CustomerPostId);
            ViewData["RequestedByUserId"] = new SelectList(_context.Users, "UserId", "UserId", job?.RequestedByUserId);
            ViewData["AssignedToUserId"] = new SelectList(_context.Users, "UserId", "UserId", job?.AssignedToUserId);
        }
    }
}
