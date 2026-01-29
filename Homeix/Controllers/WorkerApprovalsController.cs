using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;

namespace Homeix.Controllers
{
    [Authorize(AuthenticationSchemes = "HomeixAuth")]
    public class WorkerApprovalsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public WorkerApprovalsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index()
        {
            var approvals = await _context.WorkerApprovals
                .Include(w => w.User)
                .OrderByDescending(w => w.ApprovalId)
                .ToListAsync();

            return View(approvals);
        }

        // =========================
        // DOWNLOAD CSV REPORT
        // =========================
        public async Task<IActionResult> DownloadReport()
        {
            var approvals = await _context.WorkerApprovals
                .Include(w => w.User)
                .OrderByDescending(w => w.ApprovalId)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("ApprovalId,User,Notes");

            foreach (var a in approvals)
            {
                sb.AppendLine(
                    $"{a.ApprovalId}," +
                    $"\"{a.User?.FullName}\"," +
                    $"\"{a.Notes}\""
                );
            }

            return File(
                Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                "WorkerApprovalsReport.csv"
            );
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var approval = await _context.WorkerApprovals
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.ApprovalId == id);

            if (approval == null) return NotFound();

            return View(approval);
        }

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            return View();
        }

        // =========================
        // CREATE (POST) — USER ID IMPLICIT
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Notes")] WorkerApproval workerApproval)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);
            workerApproval.UserId = userId;

            if (!ModelState.IsValid)
                return View(workerApproval);

            bool exists = await _context.WorkerApprovals
                .AnyAsync(w => w.UserId == userId);

            if (exists)
            {
                ModelState.AddModelError("", "You already have a worker approval request.");
                return View(workerApproval);
            }

            _context.WorkerApprovals.Add(workerApproval);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT (GET)
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var approval = await _context.WorkerApprovals
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.ApprovalId == id);

            if (approval == null) return NotFound();

            return View(approval);
        }

        // =========================
        // EDIT (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ApprovalId,Notes")] WorkerApproval workerApproval)
        {
            if (id != workerApproval.ApprovalId)
                return NotFound();

            var existing = await _context.WorkerApprovals
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.ApprovalId == id);

            if (existing == null)
                return NotFound();

            workerApproval.UserId = existing.UserId;

            if (!ModelState.IsValid)
                return View(workerApproval);

            _context.Update(workerApproval);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DELETE (GET)
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var approval = await _context.WorkerApprovals
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.ApprovalId == id);

            if (approval == null) return NotFound();

            return View(approval);
        }

        // =========================
        // DELETE (POST)
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var approval = await _context.WorkerApprovals.FindAsync(id);

            if (approval != null)
            {
                _context.WorkerApprovals.Remove(approval);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(AuthenticationSchemes = "HomeixAuth")]
        public async Task<IActionResult> MyRequests()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var myRequests = await _context.WorkerApprovals
                .Include(w => w.User)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.ApprovalId)
                .ToListAsync();

            return View(myRequests);
        }

    }
}
