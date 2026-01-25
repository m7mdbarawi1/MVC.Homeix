using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;
using System.Text;

namespace Homeix.Controllers
{
    public class OffersController : Controller
    {
        private readonly HOMEIXDbContext _context;
        public OffersController(HOMEIXDbContext context) {_context = context;}
        public async Task<IActionResult> Index()
        {
            var offers = await _context.Offers.Include(o => o.CustomerPost).Include(o => o.User).ToListAsync();
            return View(offers);
        }
        public async Task<IActionResult> DownloadReport()
        {
            var offers = await _context.Offers.Include(o => o.CustomerPost).Include(o => o.User).OrderByDescending(o => o.CreatedAt).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("OfferId,ProposedPrice,Status,CreatedAt,CustomerPostId,UserId");
            foreach (var o in offers)
            {
                sb.AppendLine($"{o.OfferId}," + $"{o.ProposedPrice}," + $"{o.Status}," + $"{o.CreatedAt:yyyy-MM-dd}," + $"{o.CustomerPostId}," + $"{o.UserId}");
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "OffersReport.csv");
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var offer = await _context.Offers.Include(o => o.CustomerPost).Include(o => o.User).FirstOrDefaultAsync(o => o.OfferId == id);
            if (offer == null) return NotFound();
            return View(offer);
        }
        public IActionResult Create()
        {
            ReloadDropdowns();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerPostId,UserId,ProposedPrice")] Offer offer)
        {
            if (!ModelState.IsValid)
            {
                ReloadDropdowns(offer);
                return View(offer);
            }
            offer.CreatedAt = DateTime.Now;
            offer.Status = "Pending";
            _context.Offers.Add(offer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var offer = await _context.Offers.FindAsync(id);
            if (offer == null) return NotFound();
            ReloadDropdowns(offer);
            return View(offer);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OfferId,CustomerPostId,UserId,ProposedPrice,Status")] Offer offer)
        {
            if (id != offer.OfferId) return NotFound();
            if (!ModelState.IsValid)
            {
                ReloadDropdowns(offer);
                return View(offer);
            }
            var existing = await _context.Offers.AsNoTracking().FirstOrDefaultAsync(o => o.OfferId == id);
            if (existing == null) return NotFound();
            offer.CreatedAt = existing.CreatedAt;
            _context.Update(offer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var offer = await _context.Offers.Include(o => o.CustomerPost).Include(o => o.User).FirstOrDefaultAsync(o => o.OfferId == id);
            if (offer == null) return NotFound();
            return View(offer);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var offer = await _context.Offers.FindAsync(id);
            if (offer != null)
            {
                _context.Offers.Remove(offer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        private void ReloadDropdowns(Offer? offer = null)
        {
            ViewData["CustomerPostId"] = new SelectList(_context.CustomerPosts, "CustomerPostId", "CustomerPostId", offer?.CustomerPostId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", offer?.UserId);
        }
    }
}
