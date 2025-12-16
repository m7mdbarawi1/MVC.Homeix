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
    public class SubscriptionsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public SubscriptionsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: Subscriptions
        // ========================
        public async Task<IActionResult> Index()
        {
            var subs = await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User)
                .ToListAsync();

            return View(subs);
        }

        // ========================
        // GET: Subscriptions/Details
        // ========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.SubscriptionId == id);

            if (subscription == null)
                return NotFound();

            return View(subscription);
        }

        // ========================
        // GET: Subscriptions/Create
        // ========================
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // ========================
        // POST: Subscriptions/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("UserId,PlanId")]
            Subscription subscription)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(subscription);
                return View(subscription);
            }

            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.PlanId == subscription.PlanId);

            if (plan == null)
            {
                ModelState.AddModelError("", "Invalid subscription plan.");
                LoadDropdowns(subscription);
                return View(subscription);
            }

            // =========================
            // SYSTEM LOGIC
            // =========================
            subscription.StartDate = DateTime.Today;
            subscription.EndDate = DateTime.Today.AddDays(plan.DurationDays);
            subscription.Status = "Active";

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: Subscriptions/Edit
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
                return NotFound();

            LoadDropdowns(subscription);
            return View(subscription);
        }

        // ========================
        // POST: Subscriptions/Edit
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("SubscriptionId,UserId,PlanId")]
            Subscription subscription)
        {
            if (id != subscription.SubscriptionId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                LoadDropdowns(subscription);
                return View(subscription);
            }

            var existing = await _context.Subscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SubscriptionId == id);

            if (existing == null)
                return NotFound();

            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.PlanId == subscription.PlanId);

            if (plan == null)
            {
                ModelState.AddModelError("", "Invalid subscription plan.");
                LoadDropdowns(subscription);
                return View(subscription);
            }

            // Preserve system fields
            subscription.StartDate = existing.StartDate;
            subscription.EndDate = existing.StartDate.AddDays(plan.DurationDays);
            subscription.Status = existing.Status;

            _context.Update(subscription);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: Subscriptions/Delete
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.SubscriptionId == id);

            if (subscription == null)
                return NotFound();

            return View(subscription);
        }

        // ========================
        // POST: Subscriptions/Delete
        // ========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription != null)
            {
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // Helpers
        // ========================
        private void LoadDropdowns(Subscription? sub = null)
        {
            ViewData["UserId"] = new SelectList(
                _context.Users,
                "UserId",
                "FullName", // ✅ UX improvement
                sub?.UserId);

            ViewData["PlanId"] = new SelectList(
                _context.SubscriptionPlans,
                "PlanId",
                "PlanName",
                sub?.PlanId);
        }
    }
}
