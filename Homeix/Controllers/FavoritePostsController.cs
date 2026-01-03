using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Homeix.Controllers
{
    [Authorize]
    public class FavoritePostsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public FavoritePostsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // ADMIN: VIEW ALL FAVORITES
        // =====================================================
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var favorites = await _context.FavoritePosts
                .Include(f => f.User)
                .OrderByDescending(f => f.AddedAt)
                .ToListAsync();

            return View(favorites);
        }

        // =====================================================
        // DETAILS
        // =====================================================
        [Authorize(Roles = "admin")]
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

        // =====================================================
        // ⭐ ADD WORKER POST TO FAVORITES (CUSTOMER)
        // =====================================================
        [HttpPost]
        [Authorize(Roles = "customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWorkerPost(int postId)
        {
            int userId = GetUserId();

            bool exists = await _context.FavoritePosts.AnyAsync(f =>
                f.UserId == userId &&
                f.PostType == "WorkerPost" &&
                f.PostId == postId);

            if (!exists)
            {
                var favorite = new FavoritePost
                {
                    UserId = userId,
                    PostType = "WorkerPost",
                    PostId = postId,
                    AddedAt = DateTime.Now
                };

                _context.FavoritePosts.Add(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("CustomerDashboard", "Dashboard");
        }

        // =====================================================
        // ❌ REMOVE WORKER POST FROM FAVORITES (CUSTOMER)
        // =====================================================
        [HttpPost]
        [Authorize(Roles = "customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveWorkerPost(int postId)
        {
            int userId = GetUserId();

            var favorite = await _context.FavoritePosts.FirstOrDefaultAsync(f =>
                f.UserId == userId &&
                f.PostType == "WorkerPost" &&
                f.PostId == postId);

            if (favorite != null)
            {
                _context.FavoritePosts.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("CustomerDashboard", "Dashboard");
        }

        // =====================================================
        // ADMIN CREATE (KEEP FOR BACKOFFICE)
        // =====================================================
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            LoadUsersDropdown();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
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

            favoritePost.AddedAt = DateTime.Now;

            _context.FavoritePosts.Add(favoritePost);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // ADMIN EDIT
        // =====================================================
        [Authorize(Roles = "admin")]
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

        [HttpPost]
        [Authorize(Roles = "admin")]
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

            favoritePost.AddedAt = existing.AddedAt;

            _context.Update(favoritePost);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // ADMIN DELETE
        // =====================================================
        [Authorize(Roles = "admin")]
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

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "admin")]
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

        // =====================================================
        // HELPERS
        // =====================================================
        private int GetUserId()
        {
            var claim = User.FindFirst("UserId")
                ?? User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                throw new UnauthorizedAccessException();

            return int.Parse(claim.Value);
        }

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
