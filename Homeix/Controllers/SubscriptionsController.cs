using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Homeix.Controllers
{
    [Authorize]
    public class SubscriptionsController : Controller
    {
        private readonly HOMEIXDbContext _context;
        public SubscriptionsController(HOMEIXDbContext context) { _context = context; }

        // ADDED ONLY: helper to detect admin (matches your existing role naming style)
        private bool IsAdmin()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase);
        }

        // ADDED ONLY: helper to get current user id from your claim ("UserId")
        private int GetCurrentUserId()
        {
            // You currently use: User.FindFirst("UserId") in Create
            // We'll keep the same source for consistency.
            return int.Parse(User.FindFirst("UserId")!.Value);
        }

        public async Task<IActionResult> Index()
        {
            // UPDATED ONLY: Admin sees all; others see their own.
            IQueryable<Subscription> query = _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User);

            if (!IsAdmin())
            {
                int userId = GetCurrentUserId();
                query = query.Where(s => s.UserId == userId);
            }

            var subs = await query.ToListAsync();
            return View(subs);
        }

        public async Task<IActionResult> DownloadReport()
        {
            // UPDATED ONLY: Admin gets all; others get their own.
            IQueryable<Subscription> query = _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User);

            if (!IsAdmin())
            {
                int userId = GetCurrentUserId();
                query = query.Where(s => s.UserId == userId);
            }

            var subs = await query.ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SubscriptionId,User,Plan,StartDate,EndDate,Status,Price");

            foreach (var s in subs)
            {
                sb.AppendLine(
                    $"{s.SubscriptionId}," +
                    $"\"{s.User?.FullName}\"," +
                    $"\"{s.Plan?.PlanName}\"," +
                    $"{s.StartDate:yyyy-MM-dd}," +
                    $"{s.EndDate:yyyy-MM-dd}," +
                    $"{s.Status}," +
                    $"{s.Plan?.Price}"
                );
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "SubscriptionsReport.csv");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // UPDATED ONLY: restrict non-admin to own subscription
            IQueryable<Subscription> query = _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User);

            if (!IsAdmin())
            {
                int userId = GetCurrentUserId();
                query = query.Where(s => s.UserId == userId);
            }

            var subscription = await query.FirstOrDefaultAsync(s => s.SubscriptionId == id);

            if (subscription == null) return NotFound();
            return View(subscription);
        }

        public IActionResult Create()
        {
            ViewData["PlanId"] = new SelectList(_context.SubscriptionPlans.Where(p => p.IsActive), "PlanId", "PlanName");
            ViewBag.PlanData = _context.SubscriptionPlans.Where(p => p.IsActive).Select(p => new { planId = p.PlanId, planName = p.PlanName, price = p.Price, durationDays = p.DurationDays }).ToList();
            var model = new Subscription { StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(30), Status = "Active" };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlanId,StartDate,EndDate")] Subscription subscription)
        {
            if (!ModelState.IsValid)
            {
                ViewData["PlanId"] = new SelectList(_context.SubscriptionPlans.Where(p => p.IsActive), "PlanId", "PlanName", subscription.PlanId);
                return View(subscription);
            }

            int userId = GetCurrentUserId();
            subscription.UserId = userId;

            var activeSubs = await _context.Subscriptions.Where(s => s.UserId == userId && s.Status == "Active").ToListAsync();
            foreach (var sub in activeSubs)
            {
                sub.Status = "Expired";
                sub.EndDate = DateTime.Today;
            }

            subscription.Status = "Active";
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            var plan = await _context.SubscriptionPlans.FirstAsync(p => p.PlanId == subscription.PlanId);
            var payment = new Payment
            {
                UserId = userId,
                SubscriptionId = subscription.SubscriptionId,
                PaymentMethodId = 1,
                Amount = plan.Price,
                PaymentDate = DateTime.Now,
                Status = "Completed",
                Quote = $"Subscription purchase: {plan.PlanName} plan"
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // UPDATED ONLY: restrict non-admin to own subscription
            IQueryable<Subscription> query = _context.Subscriptions.AsQueryable();
            if (!IsAdmin())
            {
                int userId = GetCurrentUserId();
                query = query.Where(s => s.UserId == userId);
            }

            var subscription = await query.FirstOrDefaultAsync(s => s.SubscriptionId == id);
            if (subscription == null) return NotFound();

            ReloadDropdowns(subscription);
            return View(subscription);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SubscriptionId,UserId,PlanId,StartDate,EndDate,Status")] Subscription subscription)
        {
            if (id != subscription.SubscriptionId) return NotFound();

            // UPDATED ONLY: prevent non-admin from editing other users' subscriptions
            if (!IsAdmin())
            {
                int userId = GetCurrentUserId();
                if (subscription.UserId != userId)
                    return Forbid();
            }

            if (!ModelState.IsValid)
            {
                ReloadDropdowns(subscription);
                return View(subscription);
            }

            _context.Update(subscription);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            // UPDATED ONLY: restrict non-admin to own subscription
            IQueryable<Subscription> query = _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User);

            if (!IsAdmin())
            {
                int userId = GetCurrentUserId();
                query = query.Where(s => s.UserId == userId);
            }

            var subscription = await query.FirstOrDefaultAsync(s => s.SubscriptionId == id);
            if (subscription == null) return NotFound();

            return View(subscription);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // UPDATED ONLY: restrict non-admin to own subscription
            IQueryable<Subscription> query = _context.Subscriptions.AsQueryable();

            if (!IsAdmin())
            {
                int userId = GetCurrentUserId();
                query = query.Where(s => s.UserId == userId);
            }

            var subscription = await query.FirstOrDefaultAsync(s => s.SubscriptionId == id);

            if (subscription != null)
            {
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private void ReloadDropdowns(Subscription? sub = null)
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", sub?.UserId);
            ViewData["PlanId"] = new SelectList(_context.SubscriptionPlans, "PlanId", "PlanName", sub?.PlanId);
        }
    }
}