using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;

namespace Homeix.Controllers
{
    public class PostMediumsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public PostMediumsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: PostMediums
        // ========================
        public async Task<IActionResult> Index()
        {
            return View(await _context.PostMedia.ToListAsync());
        }

        // ========================
        // GET: PostMediums/Details
        // ========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var postMedium = await _context.PostMedia
                .FirstOrDefaultAsync(m => m.MediaId == id);

            if (postMedium == null)
                return NotFound();

            return View(postMedium);
        }

        // ========================
        // GET: PostMediums/Create
        // ========================
        public IActionResult Create()
        {
            return View();
        }

        // ========================
        // POST: PostMediums/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("PostType,PostId,MediaPath")]
            PostMedium postMedium)
        {
            if (!ModelState.IsValid)
                return View(postMedium);

            // ✅ system-managed
            postMedium.UploadedAt = DateTime.Now;

            _context.PostMedia.Add(postMedium);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: PostMediums/Edit
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var postMedium = await _context.PostMedia.FindAsync(id);
            if (postMedium == null)
                return NotFound();

            return View(postMedium);
        }

        // ========================
        // POST: PostMediums/Edit
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MediaId,PostType,PostId,MediaPath")]
            PostMedium postMedium)
        {
            if (id != postMedium.MediaId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(postMedium);

            var existing = await _context.PostMedia
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.MediaId == id);

            if (existing == null)
                return NotFound();

            // ✅ preserve system field
            postMedium.UploadedAt = existing.UploadedAt;

            _context.Update(postMedium);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: PostMediums/Delete
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var postMedium = await _context.PostMedia
                .FirstOrDefaultAsync(m => m.MediaId == id);

            if (postMedium == null)
                return NotFound();

            return View(postMedium);
        }

        // ========================
        // POST: PostMediums/Delete
        // ========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var postMedium = await _context.PostMedia.FindAsync(id);
            if (postMedium != null)
            {
                _context.PostMedia.Remove(postMedium);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
