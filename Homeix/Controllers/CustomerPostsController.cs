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
    public class CustomerPostsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public CustomerPostsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: CustomerPosts
        // ========================
        public async Task<IActionResult> Index()
        {
            var posts = await _context.CustomerPosts
                .Include(p => p.PostCategory)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        // ========================
        // GET: CustomerPosts/Details/5
        // ========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.CustomerPosts
                .Include(p => p.PostCategory)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.CustomerPostId == id);

            if (post == null) return NotFound();

            return View(post);
        }

        // ========================
        // GET: CustomerPosts/Create
        // ========================
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // ========================
        // POST: CustomerPosts/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(
            "UserId,PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax"
        )] CustomerPost customerPost)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(customerPost);
                return View(customerPost);
            }

            // system-managed fields
            customerPost.CreatedAt = DateTime.Now;
            customerPost.Status = "Open";
            customerPost.IsActive = true;

            _context.CustomerPosts.Add(customerPost);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: CustomerPosts/Edit/5
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.CustomerPosts.FindAsync(id);
            if (post == null) return NotFound();

            LoadDropdowns(post);
            return View(post);
        }

        // ========================
        // POST: CustomerPosts/Edit/5
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(
            "CustomerPostId,UserId,PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax,IsActive"
        )] CustomerPost customerPost)
        {
            if (id != customerPost.CustomerPostId) return NotFound();

            if (!ModelState.IsValid)
            {
                LoadDropdowns(customerPost);
                return View(customerPost);
            }

            // Load existing row to preserve system fields
            var existing = await _context.CustomerPosts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.CustomerPostId == id);

            if (existing == null) return NotFound();

            customerPost.CreatedAt = existing.CreatedAt;
            customerPost.Status = existing.Status;

            _context.CustomerPosts.Update(customerPost);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: CustomerPosts/Delete/5
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.CustomerPosts
                .Include(p => p.PostCategory)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.CustomerPostId == id);

            if (post == null) return NotFound();

            return View(post);
        }

        // ========================
        // POST: CustomerPosts/Delete/5
        // ========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.CustomerPosts.FindAsync(id);
            if (post != null)
            {
                _context.CustomerPosts.Remove(post);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // Helpers
        // ========================
        private void LoadDropdowns(CustomerPost? post = null)
        {
            // ✅ show FullName instead of UserId
            ViewData["UserId"] = new SelectList(
                _context.Users.AsNoTracking().OrderBy(u => u.FullName),
                "UserId",
                "FullName",
                post?.UserId
            );

            // ✅ show CategoryName instead of PostCategoryId
            ViewData["PostCategoryId"] = new SelectList(
                _context.PostCategories.AsNoTracking().OrderBy(c => c.CategoryName),
                "PostCategoryId",
                "CategoryName",
                post?.PostCategoryId
            );
        }
    }
}
