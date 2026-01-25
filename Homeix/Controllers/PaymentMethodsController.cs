using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;
using System.Text;

namespace Homeix.Controllers
{
    public class PaymentMethodsController : Controller
    {
        private readonly HOMEIXDbContext _context;
        public PaymentMethodsController(HOMEIXDbContext context) {_context = context;}
        public async Task<IActionResult> Index() {return View(await _context.PaymentMethods.ToListAsync());}
        public async Task<IActionResult> DownloadReport()
        {
            var methods = await _context.PaymentMethods.OrderBy(m => m.MethodName).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("PaymentMethodId,MethodName");
            foreach (var m in methods) { sb.AppendLine($"{m.PaymentMethodId},\"{m.MethodName}\"");}
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "PaymentMethodsReport.csv");
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var paymentMethod = await _context.PaymentMethods.FirstOrDefaultAsync(m => m.PaymentMethodId == id);
            if (paymentMethod == null) return NotFound();
            return View(paymentMethod);
        }
        public IActionResult Create() {return View();}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MethodName")] PaymentMethod paymentMethod)
        {
            if (!ModelState.IsValid) return View(paymentMethod);
            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var paymentMethod = await _context.PaymentMethods.FindAsync(id);
            if (paymentMethod == null) return NotFound();
            return View(paymentMethod);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PaymentMethodId,MethodName")] PaymentMethod paymentMethod)
        {
            if (id != paymentMethod.PaymentMethodId) return NotFound();
            if (!ModelState.IsValid) return View(paymentMethod);
            _context.Update(paymentMethod);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var paymentMethod = await _context.PaymentMethods.FirstOrDefaultAsync(m => m.PaymentMethodId == id);
            if (paymentMethod == null) return NotFound();
            return View(paymentMethod);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paymentMethod = await _context.PaymentMethods.FindAsync(id);
            if (paymentMethod != null)
            {
                _context.PaymentMethods.Remove(paymentMethod);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
