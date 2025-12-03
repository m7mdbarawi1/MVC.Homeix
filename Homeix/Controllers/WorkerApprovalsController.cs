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
    public class WorkerApprovalsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public WorkerApprovalsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // GET: WorkerApprovals
        public async Task<IActionResult> Index()
        {
            var hOMEIXDbContext = _context.WorkerApprovals.Include(w => w.ReviewedByUser).Include(w => w.User);
            return View(await hOMEIXDbContext.ToListAsync());
        }

        // GET: WorkerApprovals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workerApproval = await _context.WorkerApprovals
                .Include(w => w.ReviewedByUser)
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.ApprovalId == id);
            if (workerApproval == null)
            {
                return NotFound();
            }

            return View(workerApproval);
        }

        // GET: WorkerApprovals/Create
        public IActionResult Create()
        {
            ViewData["ReviewedByUserId"] = new SelectList(_context.Users, "UserId", "UserId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: WorkerApprovals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ApprovalId,UserId,ReviewedByUserId,Status,Notes,RequestedAt,ReviewedAt")] WorkerApproval workerApproval)
        {
            if (ModelState.IsValid)
            {
                _context.Add(workerApproval);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReviewedByUserId"] = new SelectList(_context.Users, "UserId", "UserId", workerApproval.ReviewedByUserId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", workerApproval.UserId);
            return View(workerApproval);
        }

        // GET: WorkerApprovals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workerApproval = await _context.WorkerApprovals.FindAsync(id);
            if (workerApproval == null)
            {
                return NotFound();
            }
            ViewData["ReviewedByUserId"] = new SelectList(_context.Users, "UserId", "UserId", workerApproval.ReviewedByUserId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", workerApproval.UserId);
            return View(workerApproval);
        }

        // POST: WorkerApprovals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ApprovalId,UserId,ReviewedByUserId,Status,Notes,RequestedAt,ReviewedAt")] WorkerApproval workerApproval)
        {
            if (id != workerApproval.ApprovalId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(workerApproval);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkerApprovalExists(workerApproval.ApprovalId))
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
            ViewData["ReviewedByUserId"] = new SelectList(_context.Users, "UserId", "UserId", workerApproval.ReviewedByUserId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", workerApproval.UserId);
            return View(workerApproval);
        }

        // GET: WorkerApprovals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workerApproval = await _context.WorkerApprovals
                .Include(w => w.ReviewedByUser)
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.ApprovalId == id);
            if (workerApproval == null)
            {
                return NotFound();
            }

            return View(workerApproval);
        }

        // POST: WorkerApprovals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workerApproval = await _context.WorkerApprovals.FindAsync(id);
            if (workerApproval != null)
            {
                _context.WorkerApprovals.Remove(workerApproval);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkerApprovalExists(int id)
        {
            return _context.WorkerApprovals.Any(e => e.ApprovalId == id);
        }
    }
}
