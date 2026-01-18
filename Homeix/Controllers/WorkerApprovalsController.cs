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
    public class WorkerApprovalsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public WorkerApprovalsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: WorkerApprovals
        // ========================
        public async Task<IActionResult> Index()
        {
            var approvals = await _context.WorkerApprovals
                .Include(w => w.User)
                .Include(w => w.ReviewedByUser)
                .OrderByDescending(w => w.RequestedAt)
                .ToListAsync();

            return View(approvals);
        }

        // ========================
        // DOWNLOAD REPORT (CSV)
        // ========================
        public async Task<IActionResult> DownloadReport()
        {
            var approvals = await _context.WorkerApprovals
                .Include(w => w.User)
                .Include(w => w.ReviewedByUser)
                .OrderByDescending(w => w.RequestedAt)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("ApprovalId,Status,Notes,RequestedAt,ReviewedAt,ReviewedBy,User");

            foreach (var a in approvals)
            {
                sb.AppendLine(
                    $"{a.ApprovalId}," +
                    $"{a.Status}," +
                    $"\"{a.Notes}\"," +
                    $"{a.RequestedAt:yyyy-MM-dd}," +
                    $"{a.ReviewedAt:yyyy-MM-dd}," +
                    $"\"{a.ReviewedByUser?.FullName}\"," +
                    $"\"{a.User?.FullName}\""
                );
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "WorkerApprovalsReport.csv");
        }

        // ========================
        // GET: WorkerApprovals/Details
        // ========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var approval = await _context.WorkerApprovals
                .Include(w => w.User)
                .Include(w => w.ReviewedByUser)
                .FirstOrDefaultAsync(m => m.ApprovalId == id);

            if (approval == null)
                return NotFound();

            return View(approval);
        }

        // ========================
        // GET: WorkerApprovals/Create
        // ========================
        public IActionResult Create()
        {
            LoadUsers();
            return View();
        }

        // ========================
        // POST: WorkerApprovals/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Notes")] WorkerApproval workerApproval)
        {
            if (!ModelState.IsValid)
            {
                LoadUsers(workerApproval.UserId);
                return View(workerApproval);
            }

            bool exists = await _context.WorkerApprovals.AnyAsync(w =>
                w.UserId == workerApproval.UserId &&
                w.Status == "Pending");

            if (exists)
            {
                ModelState.AddModelError("", "This user already has a pending approval request.");
                LoadUsers(workerApproval.UserId);
                return View(workerApproval);
            }

            workerApproval.Status = "Pending";
            workerApproval.RequestedAt = DateTime.Now;
            workerApproval.ReviewedAt = null;
            workerApproval.ReviewedByUserId = null;

            _context.WorkerApprovals.Add(workerApproval);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: WorkerApprovals/Edit
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var approval = await _context.WorkerApprovals.FindAsync(id);
            if (approval == null)
                return NotFound();

            LoadUsers(approval.UserId);
            return View(approval);
        }

        // ========================
        // POST: WorkerApprovals/Edit
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ApprovalId,UserId,Notes")] WorkerApproval workerApproval)
        {
            if (id != workerApproval.ApprovalId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                LoadUsers(workerApproval.UserId);
                return View(workerApproval);
            }

            var existing = await _context.WorkerApprovals
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.ApprovalId == id);

            if (existing == null)
                return NotFound();

            workerApproval.Status = existing.Status;
            workerApproval.RequestedAt = existing.RequestedAt;
            workerApproval.ReviewedAt = existing.ReviewedAt;
            workerApproval.ReviewedByUserId = existing.ReviewedByUserId;

            _context.Update(workerApproval);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: WorkerApprovals/Delete
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var approval = await _context.WorkerApprovals
                .Include(w => w.User)
                .Include(w => w.ReviewedByUser)
                .FirstOrDefaultAsync(m => m.ApprovalId == id);

            if (approval == null)
                return NotFound();

            return View(approval);
        }

        // ========================
        // POST: WorkerApprovals/Delete
        // ========================
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

        // ========================
        // Helpers
        // ========================
        private void LoadUsers(int? selectedUserId = null)
        {
            ViewData["UserId"] = new SelectList(
                _context.Users,
                "UserId",
                "FullName",
                selectedUserId
            );
        }
    }
}
