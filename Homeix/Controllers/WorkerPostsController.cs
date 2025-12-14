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
    public class WorkerPostsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public WorkerPostsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: WorkerPosts
        // ========================
        public async Task<IActionResult> Index()
        {
            var posts = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.User)
                .ToListAsync();

            return View(posts);
        }

        // ========================
        // GET: WorkerPosts/Details/5
        // ========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
                return NotFound();

            var workerPost = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.WorkerPostId == id);

            if (workerPost is null)
                return NotFound();

            return View(workerPost);
        }

        // ========================
        // GET: WorkerPosts/Create
        // ========================
        public IActionResult Create()
        {
            ViewData["PostCategoryId"] =
                new SelectList(_context.PostCategories, "PostCategoryId", "PostCategoryId");

            ViewData["UserId"] =
                new SelectList(_context.Users, "UserId", "UserId");

            return View();
        }

        // ========================
        // POST: WorkerPosts/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("UserId,PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax")]
            WorkerPost workerPost)
        {
            if (!ModelState.IsValid)
            {
                ReloadDropdowns(workerPost);
                return View(workerPost);
            }

            // ✅ System-managed fields
            workerPost.CreatedAt = DateTime.Now;
            workerPost.IsActive = true;

            _context.WorkerPosts.Add(workerPost);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: WorkerPosts/Edit/5
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
                return NotFound();

            var workerPost = await _context.WorkerPosts.FindAsync(id);

            if (workerPost is null)
                return NotFound();

            ReloadDropdowns(workerPost);
            return View(workerPost);
        }

        // ========================
        // POST: WorkerPosts/Edit/5
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("WorkerPostId,UserId,PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax,IsActive")]
            WorkerPost workerPost)
        {
            if (id != workerPost.WorkerPostId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ReloadDropdowns(workerPost);
                return View(workerPost);
            }

            var existing = await _context.WorkerPosts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.WorkerPostId == id);

            if (existing is null)
                return NotFound();

            // ✅ Preserve CreatedAt (not editable)
            workerPost.CreatedAt = existing.CreatedAt;

            _context.Update(workerPost);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: WorkerPosts/Delete/5
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
                return NotFound();

            var workerPost = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.WorkerPostId == id);

            if (workerPost is null)
                return NotFound();

            return View(workerPost);
        }

        // ========================
        // POST: WorkerPosts/Delete/5
        // ========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workerPost = await _context.WorkerPosts.FindAsync(id);

            if (workerPost is not null)
            {
                _context.WorkerPosts.Remove(workerPost);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // Helpers
        // ========================
        private void ReloadDropdowns(WorkerPost workerPost)
        {
            ViewData["PostCategoryId"] =
                new SelectList(_context.PostCategories,
                               "PostCategoryId",
                               "PostCategoryId",
                               workerPost.PostCategoryId);

            ViewData["UserId"] =
                new SelectList(_context.Users,
                               "UserId",
                               "UserId",
                               workerPost.UserId);
        }
    }
}
