using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Controllers
{
    [Authorize]
    public class SubscriptionsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public SubscriptionsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // =========================
        // HELPERS
        // =========================
        private bool IsAdmin()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase);
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst("UserId");
            if (claim == null)
                throw new UnauthorizedAccessException();

            return int.Parse(claim.Value);
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index()
        {
            IQueryable<Subscription> query = _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User);

            if (!IsAdmin())
            {
                int userId = GetCurrentUserId();
                query = query.Where(s => s.UserId == userId);
            }

            return View(await query.ToListAsync());
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            IQueryable<Subscription> query = _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User);

            if (!IsAdmin())
            {
                int userId = GetCurrentUserId();
                query = query.Where(s => s.UserId == userId);
            }

            var subscription = await query.FirstOrDefaultAsync(s => s.SubscriptionId == id);
            if (subscription == null)
                return NotFound();

            return View(subscription);
        }

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            ViewBag.Plans = _context.SubscriptionPlans
                .Where(p => p.IsActive)
                .ToList();

            return View(new Subscription());
        }

        // =========================
        // CREATE (POST) ✅ FIXED
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlanId")] Subscription subscription)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Plans = _context.SubscriptionPlans
                    .Where(p => p.IsActive)
                    .ToList();

                return View(subscription);
            }

            int userId = GetCurrentUserId();

            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.PlanId == subscription.PlanId && p.IsActive);

            if (plan == null)
                return BadRequest("Invalid subscription plan.");

            // Expire old subscriptions
            var activeSubs = await _context.Subscriptions
                .Where(s => s.UserId == userId && s.Status == "Active")
                .ToListAsync();

            foreach (var s in activeSubs)
            {
                s.Status = "Expired";
                s.EndDate = DateTime.Today;
            }

            // Create subscription
            subscription.UserId = userId;
            subscription.StartDate = DateTime.Today;
            subscription.EndDate = DateTime.Today.AddDays(plan.DurationDays);
            subscription.Status = "Active";

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // ✅ GET REAL PAYMENT METHOD (NO HARD CODE)
            var paymentMethod = await _context.PaymentMethods
                .OrderBy(pm => pm.PaymentMethodId)
                .FirstOrDefaultAsync();

            if (paymentMethod == null)
            {
                throw new InvalidOperationException(
                    "No PaymentMethod exists. Please create one first."
                );
            }

            // Create payment safely
            var payment = new Payment
            {
                UserId = userId,
                SubscriptionId = subscription.SubscriptionId,
                PaymentMethodId = paymentMethod.PaymentMethodId,
                Amount = plan.Price,
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT (GET)
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            IQueryable<Subscription> query = _context.Subscriptions;

            if (!IsAdmin())
            {
                int userId = GetCurrentUserId();
                query = query.Where(s => s.UserId == userId);
            }

            var subscription = await query.FirstOrDefaultAsync(s => s.SubscriptionId == id);
            if (subscription == null)
                return NotFound();

            ReloadDropdowns(subscription);
            return View(subscription);
        }

        // =========================
        // EDIT (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("SubscriptionId,UserId,PlanId,StartDate,EndDate,Status")]
            Subscription subscription)
        {
            if (id != subscription.SubscriptionId)
                return NotFound();

            if (!IsAdmin())
                return Forbid();

            if (!ModelState.IsValid)
            {
                ReloadDropdowns(subscription);
                return View(subscription);
            }

            _context.Update(subscription);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DELETE
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SubscriptionId == id);

            if (subscription == null)
                return NotFound();

            return View(subscription);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Get subscription with related payments
            var subscription = await _context.Subscriptions
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.SubscriptionId == id);

            if (subscription == null)
                return RedirectToAction(nameof(Index));

            // 🔥 1. DELETE PAYMENTS FIRST
            if (subscription.Payments.Any())
            {
                _context.Payments.RemoveRange(subscription.Payments);
            }

            // 🔥 2. DELETE SUBSCRIPTION
            _context.Subscriptions.Remove(subscription);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DROPDOWNS
        // =========================
        private void ReloadDropdowns(Subscription? sub = null)
        {
            ViewData["UserId"] = new SelectList(
                _context.Users,
                "UserId",
                "FullName",
                sub?.UserId
            );

            ViewData["PlanId"] = new SelectList(
                _context.SubscriptionPlans.Where(p => p.IsActive),
                "PlanId",
                "PlanName",
                sub?.PlanId
            );
        }
    }
}
