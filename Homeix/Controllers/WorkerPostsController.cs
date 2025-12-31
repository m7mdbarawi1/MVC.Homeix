using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;
using System.Security.Claims;
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
        // ADMIN: VIEW ALL WORKER POSTS
        // =====================================================
        [Authorize(Roles = "admin")]
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
        // DETAILS (Owner or Admin)
        // =====================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.WorkerPostId == id);

            if (post == null) return NotFound();

            if (!IsOwnerOrAdmin(post))
                return Forbid();

            return View(post);
        }

        // =====================================================
        // CREATE (GET)
        // =====================================================
        [Authorize(Roles = "worker")]
        public IActionResult Create()
        {
            LoadCategories();
            return View();
        }

        // =====================================================
        // CREATE (POST)
        // =====================================================
        [HttpPost]
        [Authorize(Roles = "worker")]
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
        [Authorize(Roles = "worker")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.WorkerPosts.FindAsync(id);
            if (post == null) return NotFound();

            if (!IsOwnerOrAdmin(post))
                return Forbid();

            LoadCategories(post);
            return View(post);
        }

        // =====================================================
        // EDIT (POST)
        // =====================================================
        [HttpPost]
        [Authorize(Roles = "worker")]
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

            if (!IsOwnerOrAdmin(existing))
                return Forbid();

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
        [Authorize(Roles = "worker")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.WorkerPostId == id);

            if (post == null) return NotFound();

            if (!IsOwnerOrAdmin(post))
                return Forbid();

            return View(post);
        }

        // =====================================================
        // DELETE (POST)
        // =====================================================
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "worker")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.WorkerPosts.FindAsync(id);
            if (post == null)
                return NotFound();

            if (!IsOwnerOrAdmin(post))
                return Forbid();

            _context.WorkerPosts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyPosts));
        }

        // =====================================================
        // MY POSTS (WORKER)
        // =====================================================
        [Authorize(Roles = "worker")]
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

        private bool IsOwnerOrAdmin(WorkerPost post)
        {
            if (User.IsInRole("admin"))
                return true;

            return post.UserId == GetUserId();
        }

        private void LoadCategories(WorkerPost? post = null)
        {
            ViewData["PostCategoryId"] = new SelectList(
                _context.PostCategories.AsNoTracking().OrderBy(c => c.CategoryName),
                "PostCategoryId",
                "CategoryName",
                post?.PostCategoryId
            );
        }
    }
}
