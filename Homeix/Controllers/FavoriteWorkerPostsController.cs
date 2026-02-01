using System;
using System.Linq;
using System.Threading.Tasks;
using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Controllers
{
    [Authorize]
    public class FavoriteWorkerPostsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public FavoriteWorkerPostsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // =========================
        // MY FAVORITES
        // =========================
        public async Task<IActionResult> Index()
        {
            int userId = GetUserId();

            var favorites = await _context.FavoriteWorkerPosts
                .Where(f => f.UserId == userId)
                .Include(f => f.WorkerPost)
                    .ThenInclude(p => p.PostCategory)
                .Include(f => f.WorkerPost)
                    .ThenInclude(p => p.Media)
                .OrderByDescending(f => f.FavoritePostId)
                .ToListAsync();

            return View(favorites);
        }

        // =========================
        // ADD TO FAVORITES
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int workerPostId)
        {
            int userId = GetUserId();

            bool exists = await _context.FavoriteWorkerPosts
                .AnyAsync(f => f.UserId == userId && f.WorkerPostId == workerPostId);

            if (!exists)
            {
                _context.FavoriteWorkerPosts.Add(new FavoriteWorkerPost
                {
                    UserId = userId,
                    WorkerPostId = workerPostId
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "WorkerPosts", new { id = workerPostId });
        }

        // =========================
        // REMOVE FROM FAVORITES
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int workerPostId)
        {
            int userId = GetUserId();

            var favorite = await _context.FavoriteWorkerPosts
                .FirstOrDefaultAsync(f => f.UserId == userId && f.WorkerPostId == workerPostId);

            if (favorite != null)
            {
                _context.FavoriteWorkerPosts.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // HELPERS
        // =========================
        private int GetUserId()
        {
            var claim = User.FindFirst("UserId");
            if (claim == null)
                throw new UnauthorizedAccessException();

            return int.Parse(claim.Value);
        }
    }
}
