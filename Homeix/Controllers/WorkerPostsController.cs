using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        // GET: WorkerPosts
        public async Task<IActionResult> Index()
        {
            var hOMEIXDbContext = _context.WorkerPosts.Include(w => w.PostCategory).Include(w => w.User);
            return View(await hOMEIXDbContext.ToListAsync());
        }

        // GET: WorkerPosts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workerPost = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.WorkerPostId == id);
            if (workerPost == null)
            {
                return NotFound();
            }

            return View(workerPost);
        }

        // GET: WorkerPosts/Create
        public IActionResult Create()
        {
            ViewData["PostCategoryId"] = new SelectList(_context.PostCategories, "PostCategoryId", "PostCategoryId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: WorkerPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WorkerPostId,UserId,PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax,CreatedAt,IsActive")] WorkerPost workerPost)
        {
            if (ModelState.IsValid)
            {
                _context.Add(workerPost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostCategoryId"] = new SelectList(_context.PostCategories, "PostCategoryId", "PostCategoryId", workerPost.PostCategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", workerPost.UserId);
            return View(workerPost);
        }

        // GET: WorkerPosts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workerPost = await _context.WorkerPosts.FindAsync(id);
            if (workerPost == null)
            {
                return NotFound();
            }
            ViewData["PostCategoryId"] = new SelectList(_context.PostCategories, "PostCategoryId", "PostCategoryId", workerPost.PostCategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", workerPost.UserId);
            return View(workerPost);
        }

        // POST: WorkerPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WorkerPostId,UserId,PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax,CreatedAt,IsActive")] WorkerPost workerPost)
        {
            if (id != workerPost.WorkerPostId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(workerPost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkerPostExists(workerPost.WorkerPostId))
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
            ViewData["PostCategoryId"] = new SelectList(_context.PostCategories, "PostCategoryId", "PostCategoryId", workerPost.PostCategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", workerPost.UserId);
            return View(workerPost);
        }

        // GET: WorkerPosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workerPost = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.WorkerPostId == id);
            if (workerPost == null)
            {
                return NotFound();
            }

            return View(workerPost);
        }

        // POST: WorkerPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workerPost = await _context.WorkerPosts.FindAsync(id);
            if (workerPost != null)
            {
                _context.WorkerPosts.Remove(workerPost);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkerPostExists(int id)
        {
            return _context.WorkerPosts.Any(e => e.WorkerPostId == id);
        }
    }
}
