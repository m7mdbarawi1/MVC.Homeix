using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;

namespace Homeix.Controllers
{
    public class SubscriptionPlansController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public SubscriptionPlansController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: SubscriptionPlans
        // ========================
        public async Task<IActionResult> Index()
        {
            return View(await _context.SubscriptionPlans.ToListAsync());
        }

        // ========================
        // GET: SubscriptionPlans/Details
        // ========================
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

        // ========================
        // GET: SubscriptionPlans/Create
        // ========================
        public IActionResult Create()
        {
            return View();
        }

        // ========================
        // POST: SubscriptionPlans/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("PlanName,Price,DurationDays,IsActive")]
            SubscriptionPlan subscriptionPlan)
        {
            if (!ModelState.IsValid)
                return View(subscriptionPlan);

            _context.SubscriptionPlans.Add(subscriptionPlan);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: SubscriptionPlans/Edit
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null)
                return NotFound();

            return View(plan);
        }

        // ========================
        // POST: SubscriptionPlans/Edit
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("PlanId,PlanName,Price,DurationDays,IsActive")]
            SubscriptionPlan subscriptionPlan)
        {
            if (id != subscriptionPlan.PlanId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(subscriptionPlan);

            _context.Update(subscriptionPlan);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: SubscriptionPlans/Delete
        // ========================
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

        // ========================
        // POST: SubscriptionPlans/Delete
        // ========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
    }
}
