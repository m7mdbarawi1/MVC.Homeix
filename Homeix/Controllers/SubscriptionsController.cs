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

            var subscriptions = await query.ToListAsync();
            return View(subscriptions);
        }

        // =========================
        // DOWNLOAD REPORT
        // =========================
        public async Task<IActionResult> DownloadReport()
        {
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
            ViewData["PlanId"] = new SelectList(
                _context.SubscriptionPlans.Where(p => p.IsActive),
                "PlanId",
                "PlanName"
            );

            var model = new Subscription
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(30),
                Status = "Active"
            };

            return View(model);
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("PlanId,StartDate,EndDate")] Subscription subscription)
        {
            if (!ModelState.IsValid)
            {
                ViewData["PlanId"] = new SelectList(
                    _context.SubscriptionPlans.Where(p => p.IsActive),
                    "PlanId",
                    "PlanName",
                    subscription.PlanId
                );
                return View(subscription);
            }

            int userId = GetCurrentUserId();
            subscription.UserId = userId;
            subscription.Status = "Active";

            // Expire existing subscriptions
            var activeSubs = await _context.Subscriptions
                .Where(s => s.UserId == userId && s.Status == "Active")
                .ToListAsync();

            foreach (var sub in activeSubs)
            {
                sub.Status = "Expired";
                sub.EndDate = DateTime.Today;
            }

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // Create payment (NO Status, NO Quote)
            var plan = await _context.SubscriptionPlans
                .FirstAsync(p => p.PlanId == subscription.PlanId);

            var payment = new Payment
            {
                UserId = userId,
                SubscriptionId = subscription.SubscriptionId,
                PaymentMethodId = 1,
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
            [Bind("SubscriptionId,UserId,PlanId,StartDate,EndDate,Status")] Subscription subscription)
        {
            if (id != subscription.SubscriptionId)
                return NotFound();

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

        // =========================
        // DELETE (GET)
        // =========================
        public async Task<IActionResult> Delete(int? id)
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
        // DELETE (POST)
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            IQueryable<Subscription> query = _context.Subscriptions;

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
                _context.SubscriptionPlans,
                "PlanId",
                "PlanName",
                sub?.PlanId
            );
        }
    }
}
