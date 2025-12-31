using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authorization;

namespace Homeix.Controllers
{
    [Authorize]
    public class WorkerPostsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public WorkerPostsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // VIEW ALL WORKER POSTS (ADMIN / GENERAL)
        // =====================================================
        public async Task<IActionResult> Index()
        {
            var posts = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.User)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        // =====================================================
        // DETAILS (TEMP: NO ACCESS CHECK)
        // =====================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var post = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.User)
                .Include(w => w.PostMedia)
                .FirstOrDefaultAsync(w => w.WorkerPostId == id);

            if (post == null)
                return NotFound();

            return View(post);
        }

        // =====================================================
        // CREATE (GET)
        // =====================================================
        public IActionResult Create()
        {
            LoadCategories();
            return View();
        }

        // =====================================================
        // CREATE (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(
            "PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax"
        )] WorkerPost workerPost)
        {
            if (!ModelState.IsValid)
            {
                LoadCategories(workerPost);
                return View(workerPost);
            }

            workerPost.UserId = GetUserId();
            workerPost.CreatedAt = DateTime.Now;
            workerPost.IsActive = true;

            _context.WorkerPosts.Add(workerPost);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyPosts));
        }

        // =====================================================
        // EDIT (GET)
        // =====================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var post = await _context.WorkerPosts.FindAsync(id);
            if (post == null)
                return NotFound();

            LoadCategories(post);
            return View(post);
        }

        // =====================================================
        // EDIT (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(
            "WorkerPostId,PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax,IsActive"
        )] WorkerPost workerPost)
        {
            if (id != workerPost.WorkerPostId)
                return NotFound();

            var existing = await _context.WorkerPosts
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.WorkerPostId == id);

            if (existing == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                LoadCategories(workerPost);
                return View(workerPost);
            }

            workerPost.UserId = existing.UserId;
            workerPost.CreatedAt = existing.CreatedAt;

            _context.WorkerPosts.Update(workerPost);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyPosts));
        }

        // =====================================================
        // DELETE (GET)
        // =====================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var post = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.WorkerPostId == id);

            if (post == null)
                return NotFound();

            return View(post);
        }

        // =====================================================
        // DELETE (POST)
        // =====================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.WorkerPosts.FindAsync(id);
            if (post == null)
                return NotFound();

            _context.WorkerPosts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyPosts));
        }

        // =====================================================
        // MY POSTS (WORKER)
        // =====================================================
        public async Task<IActionResult> MyPosts()
        {
            int userId = GetUserId();

            var posts = await _context.WorkerPosts
                .Where(w => w.UserId == userId)
                .Include(w => w.PostCategory)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        // =====================================================
        // HELPERS
        // =====================================================
        private int GetUserId()
        {
            var claim = User.FindFirst("UserId");
            if (claim == null)
                throw new UnauthorizedAccessException();

            return int.Parse(claim.Value);
        }

        private void LoadCategories(WorkerPost? post = null)
        {
            ViewData["PostCategoryId"] = new SelectList(
                _context.PostCategories
                    .AsNoTracking()
                    .OrderBy(c => c.CategoryName),
                "PostCategoryId",
                "CategoryName",
                post?.PostCategoryId
            );
        }
    }
}
