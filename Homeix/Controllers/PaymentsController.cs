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
    public class PaymentsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public PaymentsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: Payments
        // ========================
        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments
                .Include(p => p.PaymentMethod)
                .Include(p => p.Subscription)
                .Include(p => p.User)
                .ToListAsync();

            return View(payments);
        }

        // ========================
        // GET: Payments/Create
        // ========================
        public IActionResult Create()
        {
            ReloadDropdowns();
            return View();
        }

        // ========================
        // POST: Payments/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("UserId,SubscriptionId,PaymentMethodId,Amount")]
            Payment payment)
        {
            if (!ModelState.IsValid)
            {
                ReloadDropdowns(payment);
                return View(payment);
            }

            // =========================
            // SYSTEM FIELDS
            // =========================
            payment.PaymentDate = DateTime.Now;
            payment.Status = "Completed";

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: Payments/Edit
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            ReloadDropdowns(payment);
            return View(payment);
        }

        // ========================
        // POST: Payments/Edit
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("PaymentId,UserId,SubscriptionId,PaymentMethodId,Amount")]
            Payment payment)
        {
            if (id != payment.PaymentId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ReloadDropdowns(payment);
                return View(payment);
            }

            var existing = await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (existing == null)
                return NotFound();

            // Preserve system fields
            payment.PaymentDate = existing.PaymentDate;
            payment.Status = existing.Status;

            _context.Update(payment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: Payments/Delete
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var payment = await _context.Payments
                .Include(p => p.PaymentMethod)
                .Include(p => p.Subscription)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PaymentId == id);

            if (payment == null)
                return NotFound();

            return View(payment);
        }

        // ========================
        // POST: Payments/Delete
        // ========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // Helpers
        // ========================
        private void ReloadDropdowns(Payment? payment = null)
        {
            ViewData["UserId"] = new SelectList(
                _context.Users,
                "UserId",
                "UserId",
                payment?.UserId);

            ViewData["SubscriptionId"] = new SelectList(
                _context.Subscriptions,
                "SubscriptionId",
                "SubscriptionId",
                payment?.SubscriptionId);

            ViewData["PaymentMethodId"] = new SelectList(
                _context.PaymentMethods,
                "PaymentMethodId",
                "PaymentMethodId",
                payment?.PaymentMethodId);
        }
    }
}
