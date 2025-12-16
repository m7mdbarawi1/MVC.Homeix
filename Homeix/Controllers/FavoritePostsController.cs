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
    public class FavoritePostsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public FavoritePostsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: FavoritePosts
        // ========================
        public async Task<IActionResult> Index()
        {
            var favorites = await _context.FavoritePosts
                .Include(f => f.User)
                .ToListAsync();

            return View(favorites);
        }

        // ========================
        // GET: FavoritePosts/Details/5
        // ========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var favoritePost = await _context.FavoritePosts
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.FavoritePostId == id);

            if (favoritePost == null)
                return NotFound();

            return View(favoritePost);
        }

        // ========================
        // GET: FavoritePosts/Create
        // ========================
        public IActionResult Create()
        {
            LoadUsersDropdown();
            return View();
        }

        // ========================
        // POST: FavoritePosts/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("UserId,PostType,PostId")]
            FavoritePost favoritePost)
        {
            if (!ModelState.IsValid)
            {
                LoadUsersDropdown(favoritePost.UserId);
                return View(favoritePost);
            }

            // =========================
            // System-managed fields
            // =========================
            favoritePost.AddedAt = DateTime.Now;

            _context.FavoritePosts.Add(favoritePost);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: FavoritePosts/Edit/5
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var favoritePost = await _context.FavoritePosts.FindAsync(id);
            if (favoritePost == null)
                return NotFound();

            LoadUsersDropdown(favoritePost.UserId);
            return View(favoritePost);
        }

        // ========================
        // POST: FavoritePosts/Edit/5
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("FavoritePostId,UserId,PostType,PostId")]
            FavoritePost favoritePost)
        {
            if (id != favoritePost.FavoritePostId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                LoadUsersDropdown(favoritePost.UserId);
                return View(favoritePost);
            }

            var existing = await _context.FavoritePosts
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FavoritePostId == id);

            if (existing == null)
                return NotFound();

            // =========================
            // Preserve system fields
            // =========================
            favoritePost.AddedAt = existing.AddedAt;

            _context.Update(favoritePost);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: FavoritePosts/Delete/5
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var favoritePost = await _context.FavoritePosts
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.FavoritePostId == id);

            if (favoritePost == null)
                return NotFound();

            return View(favoritePost);
        }

        // ========================
        // POST: FavoritePosts/Delete/5
        // ========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var favoritePost = await _context.FavoritePosts.FindAsync(id);
            if (favoritePost != null)
            {
                _context.FavoritePosts.Remove(favoritePost);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // Helpers
        // ========================
        private void LoadUsersDropdown(int? selectedUserId = null)
        {
            ViewData["UserId"] = new SelectList(
                _context.Users,
                "UserId",
                "UserId",
                selectedUserId
            );
        }
    }
}
