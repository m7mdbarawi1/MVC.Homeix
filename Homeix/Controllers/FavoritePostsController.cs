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
    public class FavoritePostsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public FavoritePostsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // GET: FavoritePosts
        public async Task<IActionResult> Index()
        {
            var hOMEIXDbContext = _context.FavoritePosts.Include(f => f.User);
            return View(await hOMEIXDbContext.ToListAsync());
        }

        // GET: FavoritePosts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favoritePost = await _context.FavoritePosts
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.FavoritePostId == id);
            if (favoritePost == null)
            {
                return NotFound();
            }

            return View(favoritePost);
        }

        // GET: FavoritePosts/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: FavoritePosts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FavoritePostId,UserId,PostType,PostId,AddedAt")] FavoritePost favoritePost)
        {
            if (ModelState.IsValid)
            {
                _context.Add(favoritePost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", favoritePost.UserId);
            return View(favoritePost);
        }

        // GET: FavoritePosts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favoritePost = await _context.FavoritePosts.FindAsync(id);
            if (favoritePost == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", favoritePost.UserId);
            return View(favoritePost);
        }

        // POST: FavoritePosts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FavoritePostId,UserId,PostType,PostId,AddedAt")] FavoritePost favoritePost)
        {
            if (id != favoritePost.FavoritePostId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(favoritePost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FavoritePostExists(favoritePost.FavoritePostId))
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
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", favoritePost.UserId);
            return View(favoritePost);
        }

        // GET: FavoritePosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favoritePost = await _context.FavoritePosts
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.FavoritePostId == id);
            if (favoritePost == null)
            {
                return NotFound();
            }

            return View(favoritePost);
        }

        // POST: FavoritePosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var favoritePost = await _context.FavoritePosts.FindAsync(id);
            if (favoritePost != null)
            {
                _context.FavoritePosts.Remove(favoritePost);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FavoritePostExists(int id)
        {
            return _context.FavoritePosts.Any(e => e.FavoritePostId == id);
        }
    }
}
