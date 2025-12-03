using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Models;
using Homeix.Data;

namespace Homeix.Controllers
{
    public class CustomerPostsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public CustomerPostsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // GET: CustomerPosts
        public async Task<IActionResult> Index()
        {
            var hOMEIXDbContext = _context.CustomerPosts.Include(c => c.PostCategory).Include(c => c.User);
            return View(await hOMEIXDbContext.ToListAsync());
        }

        // GET: CustomerPosts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customerPost = await _context.CustomerPosts
                .Include(c => c.PostCategory)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CustomerPostId == id);
            if (customerPost == null)
            {
                return NotFound();
            }

            return View(customerPost);
        }

        // GET: CustomerPosts/Create
        public IActionResult Create()
        {
            ViewData["PostCategoryId"] = new SelectList(_context.PostCategories, "PostCategoryId", "PostCategoryId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: CustomerPosts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerPostId,UserId,PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax,Status,CreatedAt,IsActive")] CustomerPost customerPost)
        {
            if (ModelState.IsValid)
            {
                _context.Add(customerPost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostCategoryId"] = new SelectList(_context.PostCategories, "PostCategoryId", "PostCategoryId", customerPost.PostCategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", customerPost.UserId);
            return View(customerPost);
        }

        // GET: CustomerPosts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customerPost = await _context.CustomerPosts.FindAsync(id);
            if (customerPost == null)
            {
                return NotFound();
            }
            ViewData["PostCategoryId"] = new SelectList(_context.PostCategories, "PostCategoryId", "PostCategoryId", customerPost.PostCategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", customerPost.UserId);
            return View(customerPost);
        }

        // POST: CustomerPosts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerPostId,UserId,PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax,Status,CreatedAt,IsActive")] CustomerPost customerPost)
        {
            if (id != customerPost.CustomerPostId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customerPost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerPostExists(customerPost.CustomerPostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostCategoryId"] = new SelectList(_context.PostCategories, "PostCategoryId", "PostCategoryId", customerPost.PostCategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", customerPost.UserId);
            return View(customerPost);
        }

        // GET: CustomerPosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customerPost = await _context.CustomerPosts
                .Include(c => c.PostCategory)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CustomerPostId == id);
            if (customerPost == null)
            {
                return NotFound();
            }

            return View(customerPost);
        }

        // POST: CustomerPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customerPost = await _context.CustomerPosts.FindAsync(id);
            if (customerPost != null)
            {
                _context.CustomerPosts.Remove(customerPost);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerPostExists(int id)
        {
            return _context.CustomerPosts.Any(e => e.CustomerPostId == id);
        }
    }
}
