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
                .Include(c => c.PostCategory)
                .Include(c => c.User)
                .ToListAsync();

            return View(posts);
        }

        // ========================
        // GET: CustomerPosts/Create
        // ========================
        public IActionResult Create()
        {
            ReloadDropdowns();
            return View();
        }

        // ========================
        // POST: CustomerPosts/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "UserId," +
                "PostCategoryId," +
                "Title," +
                "Description," +
                "Location," +
                "PriceRangeMin," +
                "PriceRangeMax"
            )]
            CustomerPost customerPost)
        {
            // ✅ ModelState now works correctly
            if (!ModelState.IsValid)
            {
                ReloadDropdowns(customerPost);
                return View(customerPost);
            }

            // =========================
            // System-managed fields
            // =========================
            customerPost.CreatedAt = DateTime.Now;
            customerPost.Status = "Open";
            customerPost.IsActive = true;

            _context.CustomerPosts.Add(customerPost);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: CustomerPosts/Edit
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var post = await _context.CustomerPosts.FindAsync(id);
            if (post == null)
                return NotFound();

            ReloadDropdowns(post);
            return View(post);
        }

        // ========================
        // POST: CustomerPosts/Edit
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "CustomerPostId," +
                "UserId," +
                "PostCategoryId," +
                "Title," +
                "Description," +
                "Location," +
                "PriceRangeMin," +
                "PriceRangeMax," +
                "IsActive"
            )]
            CustomerPost customerPost)
        {
            if (id != customerPost.CustomerPostId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ReloadDropdowns(customerPost);
                return View(customerPost);
            }

            var existing = await _context.CustomerPosts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.CustomerPostId == id);

            if (existing == null)
                return NotFound();

            // ✅ Preserve system fields
            customerPost.CreatedAt = existing.CreatedAt;
            customerPost.Status = existing.Status;

            _context.Update(customerPost);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: CustomerPosts/Delete
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var post = await _context.CustomerPosts
                .Include(c => c.PostCategory)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CustomerPostId == id);

            if (post == null)
                return NotFound();

            return View(post);
        }

        // ========================
        // POST: CustomerPosts/Delete
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
        private void ReloadDropdowns(CustomerPost? post = null)
        {
            ViewData["UserId"] = new SelectList(
                _context.Users,
                "UserId",
                "UserId",
                post?.UserId
            );

            ViewData["PostCategoryId"] = new SelectList(
                _context.PostCategories,
                "PostCategoryId",
                "PostCategoryId",
                post?.PostCategoryId
            );
        }
    }
}
