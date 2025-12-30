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

        // GET: Subscriptions
        public async Task<IActionResult> Index()
        {
            var subs = await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User)
                .ToListAsync();

            return View(subs);
        }

        // GET: Subscriptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SubscriptionId == id);

            if (subscription == null) return NotFound();

            return View(subscription);
        }

        // GET: Subscriptions/Create
        public IActionResult Create()
        {
            ReloadDropdowns();
            // sensible defaults
            var model = new Subscription
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(30),
                Status = "Active"
            };
            return View(model);
        }

        // POST: Subscriptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("UserId,PlanId,StartDate,EndDate,Status")]
            Subscription subscription)
        {
            if (!ModelState.IsValid)
            {
                ReloadDropdowns(subscription);
                return View(subscription);
            }

            // (Optional) If you want EndDate auto-based on plan when empty/invalid:
            // var plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.PlanId == subscription.PlanId);
            // if (plan != null && subscription.EndDate <= subscription.StartDate)
            //     subscription.EndDate = subscription.StartDate.AddDays(plan.DurationDays);

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Subscriptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null) return NotFound();

            ReloadDropdowns(subscription);
            return View(subscription);
        }

        // POST: Subscriptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("SubscriptionId,UserId,PlanId,StartDate,EndDate,Status")]
            Subscription subscription)
        {
            if (id != subscription.SubscriptionId) return NotFound();

            if (!ModelState.IsValid)
            {
                ReloadDropdowns(subscription);
                return View(subscription);
            }

            // Ensure row exists
            var exists = await _context.Subscriptions.AnyAsync(s => s.SubscriptionId == id);
            if (!exists) return NotFound();

            _context.Update(subscription);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Subscriptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SubscriptionId == id);

            if (subscription == null) return NotFound();

            return View(subscription);
        }

        // POST: Subscriptions/Delete/5
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

        private void ReloadDropdowns(Subscription? sub = null)
        {
            ViewData["UserId"] = new SelectList(
                _context.Users,
                "UserId",
                "FullName",
                sub?.UserId
            );

            ViewData["PlanId"] = new SelectList(
                _context.SubscriptionPlans,
                "PlanId",
                "PlanName",
                sub?.PlanId
            );
        }
    }
}
