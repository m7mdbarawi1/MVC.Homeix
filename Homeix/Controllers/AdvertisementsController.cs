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
    public class AdvertisementsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public AdvertisementsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: Advertisements
        // ========================
        public async Task<IActionResult> Index()
        {
            var ads = await _context.Advertisements
                .Include(a => a.CreatedByUser)
                .ToListAsync();

            return View(ads);
        }

        // ========================
        // GET: Advertisements/Details/5
        // ========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var advertisement = await _context.Advertisements
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.AdId == id);

            if (advertisement == null)
                return NotFound();

            return View(advertisement);
        }

        // ========================
        // GET: Advertisements/Create
        // ========================
        public IActionResult Create()
        {
            LoadUsersDropdown();
            return View();
        }

        // ========================
        // POST: Advertisements/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("CreatedByUserId,Title,ImagePath,StartDate,EndDate,IsActive")]
            Advertisement advertisement)
        {
            if (!ModelState.IsValid)
            {
                LoadUsersDropdown(advertisement.CreatedByUserId);
                return View(advertisement);
            }

            _context.Advertisements.Add(advertisement);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: Advertisements/Edit/5
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement == null)
                return NotFound();

            LoadUsersDropdown(advertisement.CreatedByUserId);
            return View(advertisement);
        }

        // ========================
        // POST: Advertisements/Edit/5
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("AdId,CreatedByUserId,Title,ImagePath,StartDate,EndDate,IsActive")]
            Advertisement advertisement)
        {
            if (id != advertisement.AdId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                LoadUsersDropdown(advertisement.CreatedByUserId);
                return View(advertisement);
            }

            _context.Update(advertisement);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: Advertisements/Delete/5
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var advertisement = await _context.Advertisements
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.AdId == id);

            if (advertisement == null)
                return NotFound();

            return View(advertisement);
        }

        // ========================
        // POST: Advertisements/Delete/5
        // ========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);

            if (advertisement != null)
            {
                _context.Advertisements.Remove(advertisement);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // Helpers
        // ========================
        private void LoadUsersDropdown(int? selectedUserId = null)
        {
            ViewData["CreatedByUserId"] =
                new SelectList(_context.Users,
                               "UserId",
                               "UserId",
                               selectedUserId);
        }
    }
}
