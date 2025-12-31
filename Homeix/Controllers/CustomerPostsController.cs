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
    public class CustomerPostsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public CustomerPostsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // VIEW ALL CUSTOMER POSTS
        // =====================================================
        public async Task<IActionResult> Index()
        {
            var posts = await _context.CustomerPosts
                .Include(p => p.PostCategory)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
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

            var post = await _context.CustomerPosts
                .Include(p => p.PostCategory)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.CustomerPostId == id);

            if (post == null)
                return NotFound();

            return View(post);
        }

        // =====================================================
        // CREATE (GET)
        // =====================================================
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // =====================================================
        // CREATE (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(
            "PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax"
        )] CustomerPost customerPost)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(customerPost);
                return View(customerPost);
            }

            customerPost.UserId = GetUserId();
            customerPost.CreatedAt = DateTime.Now;
            customerPost.Status = "Open";
            customerPost.IsActive = true;

            _context.CustomerPosts.Add(customerPost);
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

            var post = await _context.CustomerPosts.FindAsync(id);
            if (post == null)
                return NotFound();

            LoadDropdowns(post);
            return View(post);
        }

        // =====================================================
        // EDIT (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(
            "CustomerPostId,PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax,IsActive"
        )] CustomerPost customerPost)
        {
            if (id != customerPost.CustomerPostId)
                return NotFound();

            var existing = await _context.CustomerPosts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.CustomerPostId == id);

            if (existing == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                LoadDropdowns(customerPost);
                return View(customerPost);
            }

            customerPost.UserId = existing.UserId;
            customerPost.CreatedAt = existing.CreatedAt;
            customerPost.Status = existing.Status;

            _context.CustomerPosts.Update(customerPost);
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

            var post = await _context.CustomerPosts
                .Include(p => p.PostCategory)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.CustomerPostId == id);

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
            var post = await _context.CustomerPosts.FindAsync(id);
            if (post == null)
                return NotFound();

            _context.CustomerPosts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyPosts));
        }

        // =====================================================
        // MY POSTS (CUSTOMER)
        // =====================================================
        public async Task<IActionResult> MyPosts()
        {
            int userId = GetUserId();

            var posts = await _context.CustomerPosts
                .Where(p => p.UserId == userId)
                .Include(p => p.PostCategory)
                .OrderByDescending(p => p.CreatedAt)
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

        private void LoadDropdowns(CustomerPost? post = null)
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
