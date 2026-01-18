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
        // DOWNLOAD REPORT (CSV)
        // ========================
        public async Task<IActionResult> DownloadReport()
        {
            var subs = await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User)
                .ToListAsync();

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

        // ========================
        // GET: Subscriptions/Details/5
        // ========================
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

        // ========================
        // GET: Subscriptions/Create
        // ========================
        public IActionResult Create()
        {
            ViewData["PlanId"] = new SelectList(
                _context.SubscriptionPlans.Where(p => p.IsActive),
                "PlanId",
                "PlanName"
            );

            ViewBag.PlanData = _context.SubscriptionPlans
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    planId = p.PlanId,
                    planName = p.PlanName,
                    price = p.Price,
                    durationDays = p.DurationDays
                })
                .ToList();

            var model = new Subscription
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(30),
                Status = "Active"
            };

            return View(model);
        }

        // ========================
        // POST: Subscriptions/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("PlanId,StartDate,EndDate")]
            Subscription subscription)
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

            int userId = int.Parse(User.FindFirst("UserId")!.Value);
            subscription.UserId = userId;

            var activeSubs = await _context.Subscriptions
                .Where(s => s.UserId == userId && s.Status == "Active")
                .ToListAsync();

            foreach (var sub in activeSubs)
            {
                sub.Status = "Expired";
                sub.EndDate = DateTime.Today;
            }

            subscription.Status = "Active";

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            var plan = await _context.SubscriptionPlans
                .FirstAsync(p => p.PlanId == subscription.PlanId);

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

        // ========================
        // GET: Subscriptions/Edit/5
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null) return NotFound();

            ReloadDropdowns(subscription);
            return View(subscription);
        }

        // ========================
        // POST: Subscriptions/Edit/5
        // ========================
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

            _context.Update(subscription);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: Subscriptions/Delete/5
        // ========================
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

        // ========================
        // POST: Subscriptions/Delete/5
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
        // HELPERS
        // ========================
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
