using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authorization;

namespace Homeix.Controllers
{
    public class SubscriptionPlansController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public SubscriptionPlansController(HOMEIXDbContext context)
        {
            _context = context;
        }
        
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.SubscriptionPlans.ToListAsync());
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DownloadReport()
        {
            var plans = await _context.SubscriptionPlans.ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("PlanId,PlanName,Price,DurationDays,MaxPostsPerMonth,IsActive");

            foreach (var p in plans)
            {
                sb.AppendLine(
                    $"{p.PlanId}," +
                    $"\"{p.PlanName}\"," +
                    $"{p.Price}," +
                    $"{p.DurationDays}," +
                    $"{(p.MaxPostsPerMonth.HasValue ? p.MaxPostsPerMonth.ToString() : "Unlimited")}," +
                    $"{p.IsActive}"
                );
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "SubscriptionPlansReport.csv");
        }

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(m => m.PlanId == id);

            if (plan == null)
                return NotFound();

            return View(plan);
        }

        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("PlanName,Price,DurationDays,IsActive,MaxPostsPerMonth")]SubscriptionPlan subscriptionPlan)
        {
            if (!ModelState.IsValid)
                return View(subscriptionPlan);

            _context.SubscriptionPlans.Add(subscriptionPlan);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null)
                return NotFound();

            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id,[Bind("PlanId,PlanName,Price,DurationDays,IsActive,MaxPostsPerMonth")] SubscriptionPlan subscriptionPlan)
        {
            if (id != subscriptionPlan.PlanId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(subscriptionPlan);

            _context.Update(subscriptionPlan);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(m => m.PlanId == id);

            if (plan == null)
                return NotFound();

            return View(plan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);

            if (plan != null)
            {
                _context.SubscriptionPlans.Remove(plan);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        
        [Authorize]
        public async Task<IActionResult> Explore()
        {
            var activePlans = await _context.SubscriptionPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .ToListAsync();

            return View(activePlans);
        }
    }
}
