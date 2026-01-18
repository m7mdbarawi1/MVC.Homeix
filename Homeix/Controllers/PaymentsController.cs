using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;
using System.Linq;
using System.Text;

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
                .Include(p => p.User)
                .Include(p => p.Subscription)
                .Include(p => p.PaymentMethod)
                .ToListAsync();

            return View(payments);
        }

        // ========================
        // DOWNLOAD REPORT (CSV)
        // ========================
        public async Task<IActionResult> DownloadReport()
        {
            var payments = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Subscription)
                .Include(p => p.PaymentMethod)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("PaymentId,Amount,PaymentDate,Status,PaymentMethod,SubscriptionId,UserId");

            foreach (var p in payments)
            {
                sb.AppendLine(
                    $"{p.PaymentId}," +
                    $"{p.Amount}," +
                    $"{p.PaymentDate:yyyy-MM-dd}," +
                    $"{p.Status}," +
                    $"{p.PaymentMethod?.PaymentMethodId}," +
                    $"{p.Subscription?.SubscriptionId}," +
                    $"{p.User?.UserId}"
                );
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "PaymentsReport.csv");
        }

        // ========================
        // GET: Payments/Details/5
        // ========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var payment = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Subscription)
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
                return NotFound();

            return View(payment);
        }

        // ========================
        // GET: Payments/Create
        // ========================
        public IActionResult Create()
        {
            LoadDropdowns();
            return View(new Payment
            {
                PaymentDate = System.DateTime.Now,
                Status = "Completed"
            });
        }

        // ========================
        // POST: Payments/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(payment);
                return View(payment);
            }

            _context.Add(payment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: Payments/Edit/5
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            LoadDropdowns(payment);
            return View(payment);
        }

        // ========================
        // POST: Payments/Edit/5
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Payment payment)
        {
            if (id != payment.PaymentId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                LoadDropdowns(payment);
                return View(payment);
            }

            _context.Update(payment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: Payments/Delete/5
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var payment = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
                return NotFound();

            return View(payment);
        }

        // ========================
        // POST: Payments/Delete/5
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
        private void LoadDropdowns(Payment? payment = null)
        {
            ViewData["UserId"] = new SelectList(
                _context.Users,
                "UserId",
                "UserId",
                payment?.UserId
            );

            ViewData["SubscriptionId"] = new SelectList(
                _context.Subscriptions,
                "SubscriptionId",
                "SubscriptionId",
                payment?.SubscriptionId
            );

            ViewData["PaymentMethodId"] = new SelectList(
                _context.PaymentMethods,
                "PaymentMethodId",
                "PaymentMethodId",
                payment?.PaymentMethodId
            );
        }
    }
}
